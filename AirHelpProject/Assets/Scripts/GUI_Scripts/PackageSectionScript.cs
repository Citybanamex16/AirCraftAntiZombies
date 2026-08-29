using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PackageSectionScript : MonoBehaviour{
[Header("References")]
public TextMeshProUGUI packageText;

public void updatePackageData(int currentValue,int maxValue){
    packageText.text = currentValue + "/" + maxValue;
}

   
}
