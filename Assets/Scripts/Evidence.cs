using UnityEngine;

public class Evidence : MonoBehaviour
{
    [Header("Evidence Information")]
    public string evidenceName;
    
    [TextArea(2, 5)]
    public string description;

    public string evidenceType;

    public bool isImportant = true;
}