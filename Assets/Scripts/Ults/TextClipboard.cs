using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextClipboard : MonoBehaviour
{
    public void CopyToClipboard(TextMeshProUGUI CopyUI)
    {
        GUIUtility.systemCopyBuffer = CopyUI.text;
        Debug.Log("Copied '" + CopyUI.text + "' to clipboard.");
    }
    
    public void PasteFromClipboard(TMP_InputField CopyUI)
    {
        CopyUI.text = GUIUtility.systemCopyBuffer;
        Debug.Log("Pasted '" + CopyUI.text + "' from clipboard.");
    }
}
