// Assets/Scripts/PhraseData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "PhraseData", menuName = "Game/Phrase Data")]
public class PhraseData : ScriptableObject
{
    [System.Serializable]
    public class Phrase
    {
        public string text;
        public bool isGood;
    }
    public Phrase[] phrases;
}