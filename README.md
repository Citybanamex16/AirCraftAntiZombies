# AirZ: Zombie Hunter

**Evidencia 2 — Actividad Integradora de Gráficas**
Modelación de Sistemas Multiagentes con Gráficas Computacionales (Gpo 101)

**Autor:** Carlos Delgado Contreras — A01712819
**Fecha:** 30 de agosto de 2026

---

## Descripción

AirZ: Zombie Hunter es un juego desarrollado en Unity en el que el jugador defiende desde las alturas a los supervivientes de un ataque zombie. Controlando la torreta de un avión de combate que orbita el escenario, el jugador debe lanzar paquetes de ayuda y eliminar a las hordas zombie antes de que estos destruyan dichos paquetes.

**Plataforma:** PC & WebGL

### Controles

| Acción | Control |
|---|---|
| Mover cámara | Mouse / TrackPad |
| Disparar | Click izquierdo / botón X |
| Aumentar zoom | Click derecho / botón Z |
| Cambiar de modo | Barra espaciadora |

## Objetivo del juego

El jugador debe lanzar paquetes de ayuda desde el avión y, una vez en el suelo, defenderlos con su arma hasta eliminar a la horda zombie correspondiente. La partida se pierde si el jugador se queda sin vidas tras la destrucción de los paquetes, y se gana al eliminar exitosamente todas las hordas requeridas.

## Sistemas técnicos destacados

El proyecto resolvió, entre otros, los siguientes retos técnicos:

- **Control de cámara sin Gimbal Lock:** separación de los ejes de rotación (yaw/pitch) mediante una jerarquía de objetos, evitando el comportamiento errático de la cámara de la torreta.
- **Object Pooling de proyectiles:** sistema de reutilización de instancias de balas para sostener la alta cadencia de disparo sin penalizar el rendimiento.
- **Gestión de enemigos con estructuras de datos:** uso combinado de `Queue` (zombies inactivos) y `HashSet` (zombies activos) junto con un patrón Singleton/Autoload, permitiendo que el Spawn Manager detecte con precisión cuándo una horda ha sido eliminada por completo.

Adicionalmente, se implementaron sistemas propios de órbita de avión, temperatura de arma, bloom de disparo, pathfinding de zombies, un mini motor de comportamiento tipo Reynolds y zoom de cámara.

## Uso de Inteligencia Artificial

En el desarrollo se utilizaron herramientas de IA (Claude y Gemini) principalmente como apoyo de aprendizaje y guía en áreas ajenas de experiencia previa (efectos visuales, Art Tech de Unity, manejo de Quaternions), así como en la codificación directa de ciertas funcionalidades puntuales (sistema de referencias por Target Channel, conversión de coordenadas 3D a 2D, y fórmulas de bloom/camera shake). Los sistemas centrales del juego fueron diseñados y programados por el autor.

## Créditos de assets

Se utilizaron recursos de terceros (modelos, sonidos y efectos) bajo licencias estándar de Unity Asset Store, MIT y Creative Commons (Attribution). El detalle completo de cada recurso, autor y licencia se encuentra en el documento adjunto.

## Playtest

A partir de retroalimentación obtenida en pruebas con un jugador externo, se agregó una sección de GUI de progreso (paquetes defendidos y vida) y se ajustó el flujo para que, tras defender un paquete, el jugador pase automáticamente al modo de lanzamiento del siguiente.

## Enlaces

- **Video de demostración:** [Ver en Google Drive](https://drive.google.com/file/d/1eUebc16TfF7nX30q_K0vsSO-6xLFwlVC/view?usp=sharing)
- **Repositorio de código fuente:** [GitHub](https://github.com/Citybanamex16/AirCraftAntiZombies.git)

---

> 📄 Este README es únicamente un resumen del proyecto. Para consultar el detalle completo de decisiones técnicas, código, créditos de assets, playtest y reflexión, revisa el documento adjunto: *Evidencia 2. Actividad Integradora de Gráficas*.