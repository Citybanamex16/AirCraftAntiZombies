using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshipTurret : MonoBehaviour
{

    [Header("Referencias")]
    public Transform PlayerCamara;
    public GameObject SpawnPoint;
    public GameObject BulletContainer;
    public GameObject PackageContainer;
    public GameObject destelloDisparo;

    [Header("Configuración de Sensibilidad")]
    public float sensibilidad = 100f;

    [Header("Límites de la Torreta (Grados Locales)")]
    public float minX = -150f;  // Izquierda (Yaw)
    public float maxX = -30f;   // Derecha (Yaw)

    // Límites verticales corregidos para la lógica de Unity
    public float minY = -20f;   // Hacia abajo (Pitch)
    public float maxY = 70f;    // Hacia arriba (Pitch)
    // Acumuladores de rotación local
    private float currentRotationY = 0f; 
    private float currentRotationX = 0f; 

    [Header("Shooting Vars")]
    public float fireRate = 0.30f;
    private float currentTime = 0.0f;

    [Header("Gatling Temp")]
    private float maxTemp = 100.0f;
    private float currentTemp = 0.0f;
    public float tempPerBullet = 1.0f;
    public float decreaseTempStep = 25.0f;
    private bool overload = false;

    [Header("Gatling Thermal Bloom")]
    public float minBloomAngle = 0.25f;   // Dispersión base (cañón frío)
    public float maxBloomAngle = 3.0f;   // Dispersión máxima (cañón sobrecalentado)
    public bool useInspectorCurve = false;
    public AnimationCurve customBloomCurve; 
    

    [Header("Camara Shake")]
    private Vector3 defaultCamaraPosition;

    public float shakeDuration = 0.15f; // Duración corta para que responda rápido a cada disparo
    private float shakeTimer = 0.0f;

    public float baseShakeIntensity = 0.05f;  // Temblor mínimo (cañón frío)
    public float maxShakeIntensity = 0.35f;   // Temblor máximo (cañón sobrecalentado)
    public float recoverySpeed = 12.0f;        // Velocidad de retorno al centro

    private bool isShaking = false;

    private Vector2 lastBloomOffset;

    [Header("Zoom vars")]
    public float zoomStep = 10.0f;
    private float currentZoom = 0.0f; //Se asigna en Start()
    private float defaultZoom = 0.0f; //Se asigna en Start()
    public float maxZoom = 30.0f;

    private Camera miCamara;


    [Header("Bullet Prefabs")]
    public bullet bulletPrefab;
    public Package packagePrefab;

    [Header("ObjectPooling Vars")]
    public int InitialPoolSize = 50;
    private Queue<bullet> SleepingBulletPool = new Queue<bullet>();

    [Header("Gatling Spin Effect")]
    public Transform minigunEntireAsset; 
    public float maxSpinSpeed = 1500f;   // Velocidad máxima de giro (grados/sec)
    public float acceleration = 2500f;   // Qué tan rápido acelera
    public float deceleration = 1800f;   // Qué tan rápido frena al soltar
    private float currentSpinSpeed = 0f;
    private bool isFiring = false;

    [Header("Configuración de Audio SFX")]
    public AudioSource gatlingAudioSource; // AudioSource 2D en la cámara o torreta
    public AudioClip gatlingLoopSound;     // 2. Gatling disparando (Loop)
    public AudioClip gatlingWindDownSound; // 3. Gatling dejando de disparar
    public AudioClip groundHitSound;       // 4. Hit de piso
    private bool wasFiringLastFrame = false;

    [Header("Debug")]
    public bool canMouse = true;

    // === Automata de Estados de torreta === //
    public enum EstadoTorreta 
    {
    Armed,
    Auxiliary
    }

    private bool canSwitch = true;


    public EstadoTorreta estadoActual;


    // Start is called before the first frame update
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        initializeObjectPooling();

        estadoActual = EstadoTorreta.Auxiliary;

        //Camera ref 
        miCamara = Camera.main;
        defaultZoom = miCamara.fieldOfView;
        currentZoom = defaultZoom;

        defaultCamaraPosition = miCamara.transform.localPosition;

          // Escribimos la ruta exacta bajando por cada hijo desde TorretaPlayer
        string rutaParticula = "Main Camera/Minigun4/ShootSpawnPoint/WFX_MF FPS RIFLE";
        
        Transform particulaTransform = transform.Find(rutaParticula);

        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(canMouse){
            aim(Time.deltaTime);
        }
        

        shoot(Time.deltaTime);

        if(canSwitch){
            if(Input.GetButton("Switch")){
                //print("¡SWITCHED!");
                if(estadoActual == EstadoTorreta.Armed){
                    //print("Gunship: Auxiliary!");
                    estadoActual = EstadoTorreta.Auxiliary;
                }
                else{
                    //print("Gunship: ¡ARMED!");
                    estadoActual = EstadoTorreta.Armed;
                }
            }
        }

        zoom(Time.deltaTime);

        CamaraShake(Time.deltaTime);

        //gatllingAnimation(Time.deltaTime);

        gatlingAudio();
        
        

    }

    private void gatllingAnimation(float delta){
        if (minigunEntireAsset == null) return;

        // 1. Acelerar si estamos disparando, o desacelerar si soltamos el gatillo
        if (isFiring)
        {
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, maxSpinSpeed, acceleration * delta);
        }
        else
        {
            currentSpinSpeed = Mathf.MoveTowards(currentSpinSpeed, 0f, deceleration * delta);
        }

        // 2. Aplicar la rotación continua sobre el eje frontal local
        if (currentSpinSpeed > 0f)
        {
            // Cambia Vector3.forward por Vector3.up o Vector3.right si tu modelo viene rotado de fábrica
            minigunEntireAsset.Rotate(Vector3.up * currentSpinSpeed * delta, Space.Self);        }

    }


    private void gatlingAudio(){
        // 1. Inicio de ráfaga
        if (isFiring && !wasFiringLastFrame)
        {
            gatlingAudioSource.clip = gatlingLoopSound;
            gatlingAudioSource.loop = true;
            gatlingAudioSource.Play();
        }
        // 2. Fin de ráfaga 
        else if (!isFiring && wasFiringLastFrame)
        {
            gatlingAudioSource.Stop();
            
            // Reproducir el sonido de frenado una sola vez 
            if (gatlingWindDownSound != null)
            {
                gatlingAudioSource.PlayOneShot(gatlingWindDownSound);
            }
        }

        wasFiringLastFrame = isFiring;

    }


    //Método público para llamar el impacto en tierra 
    public void PlayGroundHitSFX(Vector3 impactPoint)
    {
        if (groundHitSound == null) return;

        // PlayClipAtPoint crea un AudioSource temporal en el punto 3D del mundo, 
        // reproduce el sonido atenuado por la distancia a la cámara y se autodestruye.
        AudioSource.PlayClipAtPoint(groundHitSound, impactPoint, 0.8f);
    }

    

    //Llamado al dispar
    public void TriggerShake(Quaternion currentBloomRotation){
        isShaking = true;
        shakeTimer = shakeDuration; // Reinicia el tiempo sin perder la posición original

        // Extraemos los ángulos de desviación del Bloom para orientar el temblor
        Vector3 euler = currentBloomRotation.eulerAngles;
        
        // == Mejora realizada con IA ==
        // Normalizamos los ángulos de Euler (-180 a 180) para obtener la dirección exacta
        float dirX = Mathf.DeltaAngle(0, euler.y); // Yaw -> Desplazamiento en X
        float dirY = Mathf.DeltaAngle(0, euler.x); // Pitch -> Desplazamiento en Y

        lastBloomOffset = new Vector2(dirX, dirY);

        // == FIN de Mejora realizada con IA ==
    }

   private void CamaraShake(float delta){
    if (miCamara == null) return;

    if (isShaking)
    {
        shakeTimer -= delta;

        if (shakeTimer <= 0)
        {
            shakeTimer = 0f;
            isShaking = false;
        }

    // === Mejora generada con IA === //
        float heatRatio = Mathf.Clamp01(currentTemp / maxTemp);
        float bloomFactor = Mathf.Pow(heatRatio, 3);
        float currentIntensity = Mathf.Lerp(baseShakeIntensity, maxShakeIntensity, bloomFactor);

        // B) Sincronizar con el Bloom: Usamos la dirección del disparo + Ruido de alta frecuencia
        float noiseX = (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * 2f;

        // Combinamos la dirección del Bloom con el temblor de la ametralladora
        Vector3 targetOffset = new Vector3(
            (lastBloomOffset.x * 0.1f + noiseX) * currentIntensity,
            (lastBloomOffset.y * 0.1f + noiseY) * currentIntensity,
            0f
        );

    // === Fin de Mejora generada con IA === //

        //C) Aplicar el offset SIEMPRE desde la defaultCamaraPosition en un Lerp.
        miCamara.transform.localPosition = Vector3.Lerp(miCamara.transform.localPosition, defaultCamaraPosition + targetOffset, delta * 25f);
    }
    else if (miCamara.transform.localPosition != defaultCamaraPosition)
    {
        // Regreso suave a la posición neutral original
        miCamara.transform.localPosition = Vector3.Lerp(miCamara.transform.localPosition, defaultCamaraPosition, delta * recoverySpeed);
    }
}


    void initializeObjectPooling(){
        for(int i = 0; i< InitialPoolSize; i++){
            bullet nuevaBala = Instantiate(bulletPrefab,Vector3.zero, Quaternion.identity);
            nuevaBala.transform.SetParent(BulletContainer.transform);
            nuevaBala.deactivate();
            nuevaBala.onBulletDeactivated += RegresarBalaAlPool;
            //La registramos en el Object Pooling :)
            SleepingBulletPool.Enqueue(nuevaBala);
        }

    }


    //Esta funcion es llamada por AirCraft (Padre)
    public void InicializarAngulos(float yawInicial, float pitchInicial){
    print("Initialized Turret Position");
    
    // 1. Seteamos los acumuladores
    currentRotationY = yawInicial; // Recibe -90f
    
    // Si la cámara mira al revés al iniciar, multiplicamos por -1 para invertir el signo del ángulo del triángulo
    currentRotationX = pitchInicial; 

    // 2. Aplicamos un Clamp inmediato 
    currentRotationY = Mathf.Clamp(currentRotationY, minX, maxX);
    currentRotationX = Mathf.Clamp(currentRotationX, minY, maxY);

    // 3. Aplicamos las rotaciones iniciales directamente a los componentes
    transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
    if (PlayerCamara != null)
    {
        PlayerCamara.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
    }
}

    //Llamada en Update() cada Frame
    void aim(float delta){

        //1. Leemos los inputs del mouse
        float mouseRawX = Input.GetAxis("Mouse X") * sensibilidad * delta;
        float mouseRawY = Input.GetAxis("Mouse Y") * sensibilidad * delta;
        //print("Current Y rotation: " + mouseRawY);
        //print("Current X rotation: " + mouseRawX);


        //2. Los acumulamos y limitamos
        currentRotationX -= mouseRawY;
        currentRotationX = Mathf.Clamp(currentRotationX,minY,maxY);

        currentRotationY += mouseRawX;
        currentRotationY = Mathf.Clamp(currentRotationY,minX,maxX);

        //3. Aplicar la rotacion en coordenadas locales
        transform.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
        //print("Local rotation: " + transform.localRotation);

        // La cámara rota sobre el eje X del pivot (arriba/abajo)
        if (PlayerCamara != null)
        {
            PlayerCamara.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);
            //print("Camera Rotation: " + PlayerCamara.localRotation);
        }


    }

    private int num = 0;

    void shoot(float delta){

        if(estadoActual == EstadoTorreta.Armed){

                if(currentTime > 0){
                currentTime -= delta;
            }

            if(Input.GetButton("Fire1")){
                if(currentTime <= 0 && currentTemp < maxTemp && !overload){
                    num += 1;
                    bullet newBullet;
                    Quaternion thermalBloom = GetThermalBloomRotation();
                    if(SleepingBulletPool.Count > 0){
                        newBullet = SleepingBulletPool.Dequeue();
                        newBullet.activate(SpawnPoint.transform.position, thermalBloom);

                    } else{
                        //AutoEscalamiento
                        newBullet = Instantiate(bulletPrefab,Vector3.zero, Quaternion.identity);
                        newBullet.transform.SetParent(BulletContainer.transform);

                        //print("Spawn point en: " + SpawnPoint.transform.position);
                        newBullet.activate(SpawnPoint.transform.position, thermalBloom);

                    }

                    //Activar particula si es que hay
                    if (destelloDisparo != null)
                    {
                        // Buscamos las partículas dentro del objeto arrastrado
                        ParticleSystem ps = destelloDisparo.GetComponent<ParticleSystem>();
                        if (ps != null) ps.Play();
                    }
                    else{

                        Debug.LogError("Muffle error: referencia incorrecta: " + destelloDisparo);
                    }

                    //print("Object Pool: " + SleepingBulletPool.Count);
                    isFiring = true;
                    currentTime = fireRate;
                    currentTemp += tempPerBullet;
                    //TriggerShake(thermalBloom);
                    //print("Current temp: " + currentTemp);

                    if(currentTemp >= maxTemp){
                        //Penalizacion
                        currentTemp = maxTemp;
                        //print("¡Overload!");
                        overload = true;
                    }
                }
                
            }
            else if(currentTemp > 0){
                currentTemp -= delta * decreaseTempStep;
                if(currentTemp <= 0){
                    overload = false;
                    currentTemp = 0;
                }
                //print(" decrasing Current temp: " + currentTemp);
                isFiring = false;
            }


        }
        else if(estadoActual == EstadoTorreta.Auxiliary){
            if(Input.GetButton("Fire1")){
                Package newPackage = Instantiate(packagePrefab,SpawnPoint.transform.position, PlayerCamara.transform.rotation);
                newPackage.transform.SetParent(PackageContainer.transform);
                estadoActual = EstadoTorreta.Armed;
                // Despues de lanzar un paquete ya no puedes lanzar mas hasta que Game Master te deje
                canSwitch = false;
            }

        }

        
    }


    //Funcion refinada y generada con IA // 
    public Quaternion GetThermalBloomRotation(){
    // 1. Ratio de temperatura de 0 a 1
    float heatRatio = Mathf.Clamp01(currentTemp / maxTemp);

    // 2. Calcular la curva de dispersión
    float bloomFactor;
    
    if (useInspectorCurve && customBloomCurve != null)
    {
        bloomFactor = customBloomCurve.Evaluate(heatRatio);
    }
    else
    {
        // Curva Cúbica: El bloom se mantiene bajo al inicio y explota al final
        bloomFactor = Mathf.Pow(heatRatio, 3); 
    }

    // 3. Interpolar la dispersión angular
    float currentBloom = Mathf.Lerp(minBloomAngle, maxBloomAngle, bloomFactor);

    // 4. Cono circular 3D aleatorio
    Vector2 randomCircle = Random.insideUnitCircle * currentBloom;
    Quaternion bloomOffset = Quaternion.Euler(randomCircle.x, randomCircle.y, 0f);

    return PlayerCamara.transform.rotation * bloomOffset;
}


    void zoom(float delta){
        if(Input.GetButton("Zoom1")){
            //Aumentar zoom progresivamente hasta el tope (Disminuir field)
            if(currentZoom > maxZoom){
                currentZoom -= delta * zoomStep;

                if (currentZoom < maxZoom) currentZoom = maxZoom; 
                print("Zooming: " + currentZoom);
                miCamara.fieldOfView = currentZoom;
            }
            
        }
        //Devolver el zoom al default si dejamos de apuntar
        else if(currentZoom < defaultZoom){
            currentZoom += (zoomStep * 2f) * delta; 

            if (currentZoom > defaultZoom) currentZoom = defaultZoom;
            //Asignar nuevo zoom
            miCamara.fieldOfView = currentZoom;
        }

        
    }

    //Estas balas ya llegan despues de hacer deactivate();
    void RegresarBalaAlPool(bullet balaDormida){
        SleepingBulletPool.Enqueue(balaDormida);
        //print("Object Pool: " + SleepingBulletPool.Count);
    }

    //Llamado por Game master
    public void reset(){
        //print("GunShip: Can Switch");
        estadoActual = EstadoTorreta.Auxiliary;
        canSwitch = true;
    }


    //Getters

    public bool getOverload(){
        return overload;
    }

    public float getMaxTemp(){
        return maxTemp;
    }

    public float getTemp(){
        return currentTemp;
    }
}
