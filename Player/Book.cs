using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AC
{
    public class Book : MonoBehaviour
    {
        public BookType bookType;
        public TMP_Text text;
        public int codeInt;
        
        private static Dictionary<BookType, int> bookCodes = new Dictionary<BookType, int>();

        public enum BookType
        {
            Red,
            Orange,
            Yellow,
            Green,
            Blue
        }

        private void Start()
        {
            if (!bookCodes.ContainsKey(bookType))
            {
                bookCodes[bookType] = GenerateRandomCode();
            }
            text.text = bookCodes[bookType].ToString();
            codeInt = bookCodes[bookType];
        }

        private int GenerateRandomCode()
        {
            return Random.Range(1, 10);
        }
    }
}