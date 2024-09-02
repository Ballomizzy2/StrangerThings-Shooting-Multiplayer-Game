using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CharacterManager : MonoBehaviour
{
    private List<Component> allMainComponents = new List<Component>();
    [SerializeField]
    private LookAtConstraint chestConstraint;

    [SerializeField]
    private Transform cameraRoot, UI;

    [SerializeField]
    public GameObject chest, rightArm;

    [SerializeField]
    public GameObject weapons, inventory;

    private StarterAssets.ThirdPersonController thirdPerson;


    private void Start()
    {
        GetAllComponents();
    }


    private void GetAllComponents()
    {
       /* Component[] components = GetComponents<Component>();
        foreach (Component comp in components) 
        {
            allMainComponents.Add(comp);
        }*/

        //Chest get
        //chestConstraint = chest.GetComponent<LookAtConstraint>();
        thirdPerson = GetComponent<StarterAssets.ThirdPersonController>();
        
    }

    public void AddMyComponentsToAnotherCharacter(CharacterManager character)
    {
        /*foreach (Component comp in allMainComponents)
        {
            character.gameObject.AddComponent(comp.GetType());
            Component comp1 = character.gameObject.GetComponent(comp.GetType());
            comp1 = comp;
        }*/

        //Chest
        LookAtConstraint c = character.chest.AddComponent<LookAtConstraint>();
        c.AddSource(chestConstraint.GetSource(0));
        c.weight = 1;
        c.constraintActive = true;
        c.rotationOffset = chestConstraint.rotationOffset;
        //Kinda like  a special occasion for imported characters
        c.rotationOffset = new Vector3(32.1f, c.rotationOffset.y, c.rotationOffset.z);

        //Wepaons placing
        //character.weapons = weapons;
        //Quaternion rot = weapons.transform.localRotation;
        //Vector3 pos = weapons.transform.localPosition;

        //character.weapons.transform.parent = character.rightArm.transform;
        //character.weapons.transform.localPosition = pos;
        //character.weapons.transform.localRotation = rot;
        //character.weapons.transform.localEulerAngles = new Vector3(68.3812561f, 19.3328476f, 166.609528f);

        //Inventory placing
        character.inventory = inventory;
        WeaponManager weaponsManager = character.weapons.GetComponent<WeaponManager>();
        character.inventory.GetComponent<Inventory>().ChangeCharacter(weaponsManager.GetInput(), weaponsManager);
        Vector3 rot1 = inventory.transform.eulerAngles;
        Vector3 pos1 = inventory.transform.localPosition;

        character.inventory.transform.parent = character.rightArm.transform;
        character.inventory.transform.localPosition = pos1;
        character.inventory.transform.eulerAngles = rot1;


        //Necessary Children Swap
        character.cameraRoot = cameraRoot;
        character.UI = UI;

        character.cameraRoot.transform.parent = character.transform;
        character.UI.transform.parent = character.transform;

        //New thirdperson details = old third person details
        //character.GetComponent<StarterAssets.ThirdPersonController>().SetNewThirdPersonProperties(thirdPerson.GetThirdPersonProperties());

    }
}
