using System.Collections;
using TMPro;
using UnityEngine;

namespace AC
{
    public class Padlock : MonoBehaviour
    {
        public TMP_InputField[] inputFields;
        public TMP_Text feedbackText;
        public Player player;

        [SerializeField] private string code;
        private int[] bookCodes = new int[5];

        private void Start()
        {
            foreach (TMP_InputField inputField in inputFields)
            {
                inputField.text = "";
            }
            feedbackText.text = "";
        }

        public void CheckCode()
        {
            string enteredCode = string.Concat(inputFields[0].text, inputFields[1].text, inputFields[2].text, inputFields[3].text, inputFields[4].text);

            if (enteredCode == code)
            {
                feedbackText.text = "Unlocked!";
                UnlockPadlock();
            }
            else
            {
                feedbackText.text = "Incorrect code. Try again.";
            }
        }

        public void HidePadlock() {
            player.passcode.SetActive(false);
            player.UpdateCursor(false);
        }

        private void UnlockPadlock()
        {
            player.NextLevel(player.transform);
            HidePadlock();
        }

        public void FindBooks()
        {
            Book[] books = FindObjectsOfType<Book>();

            foreach (Book book in books)
            {
                switch (book.bookType)
                {
                    case Book.BookType.Red:
                        bookCodes[0] = book.codeInt;
                        break;
                    case Book.BookType.Orange:
                        bookCodes[1] = book.codeInt;
                        break;
                    case Book.BookType.Yellow:
                        bookCodes[2] = book.codeInt;
                        break;
                    case Book.BookType.Green:
                        bookCodes[3] = book.codeInt;
                        break;
                    case Book.BookType.Blue:
                        bookCodes[4] = book.codeInt;
                        break;
                }
            }

            code = string.Concat(bookCodes[0], bookCodes[1], bookCodes[2], bookCodes[3], bookCodes[4]);
        }
    }
}