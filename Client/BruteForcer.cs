using Core;

namespace Droch1;

    class BruteForcer
    {
        private string login;
        private List<char> alphabet;


        public BruteForcer(string login)
        {
            this.login = login;
        }

        public IEnumerable<LoginInfo> BruteForce()
        {
            bool cracked = false;

            List<char> passwordAttempt = new List<char>();

            alphabet = GetAlphabet();

            passwordAttempt.Add(alphabet[0]); // First character to be checked (if there is a minimum length, add more 0s to the list)

            while (true)
            {
                // Get the character that needs to be updated
                int index = GetUpdateIndex(passwordAttempt.Count - 1, passwordAttempt);

                if (index != -1)
                {
                    // Update the characters in the password attempt
                    UpdatePasswordAttempt(index, ref passwordAttempt);
                }
                else
                {
                    // Add a new character to the list and set all of the letter to the first ascii value
                    AddNewCharacter(ref passwordAttempt);
                }
                
                yield return new LoginInfo(login, new string(passwordAttempt.ToArray()));
            }
        }


        /// <summary>
        /// Add a new character to the list and set all of the existing values to the first ascii value in the ascii list
        /// </summary>
        /// <param name="letters"> the list of letters that make up the password attempt </param>
        private void AddNewCharacter(ref List<char> letters)
        {
            List<char> ascii = GetAlphabet();

            letters.Add('0');

            for (int i = 0; i < letters.Count; i++)
            {
                letters[i] = ascii[0];
            }
        }


        /// <summary>
        /// Change the character at the intended index and if there are any characters that come after the index, change those to the first ascii value
        /// </summary>
        /// <param name="index"> position of character being changed </param>
        /// <param name="letters"> List of characters that make up the password attempt </param>
        private void UpdatePasswordAttempt(int index, ref List<char> letters)
        {
            if (index != letters.Count - 1)
            {
                for (int i = index + 1; i < letters.Count; i++)
                {
                    letters[i] = alphabet[0];
                }
            }

            letters[index]++;
        }


        /// <summary>
        ///  Recursive function used for checking which letter needs to be updated
        ///  If there are no letters that can be updated, a character will need to be added to the list
        /// </summary>
        /// <param name="index"> the last character in the list </param>
        /// <param name="letters"> the list of attempted characters </param>
        /// <returns> the index of the character that needs to be incremented, -1 if a new character needs to be added </returns>
        private int GetUpdateIndex(int index, List<char> letters)
        {
            if (letters[index] == alphabet[^1])
            {
                if (index != 0)
                {
                    index = GetUpdateIndex(index - 1, letters);
                }
                else
                    return -1;
            }

            return index;

        }


        List<char> GetAlphabet()
        {
            List<char> asciiLetters = new List<char>();

            char letter = '0';
            letter--;
            for (int i = 0; i < 81; i++)
            {
                asciiLetters.Add(letter);
                letter++;
            }

            return asciiLetters;
        }
    }