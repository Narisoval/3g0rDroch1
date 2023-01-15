using Core;

namespace Droch1;

    class BruteForcer
    {
        private readonly string _login;
        private List<char> _alphabet;
        private List<char> _passwordAttempt;
        public BruteForcer(string login)
        {
            _login = login;
        }

        public IEnumerable<LoginInfo> BruteForce()
        {
            _passwordAttempt = new List<char>();

            _alphabet = GetAlphabet();

            _passwordAttempt.Add(_alphabet[0]); // First character to be checked (if there is a minimum length, add more 0s to the list)

            while (true)
            {
                // Get the character that needs to be updated
                int index = GetUpdateIndex(_passwordAttempt.Count - 1);

                if (index != -1)
                {
                    // Update the characters in the password attempt
                    UpdatePasswordAttempt(index);
                }
                else
                {
                    // Add a new character to the list and set all of the letter to the first ascii value
                    AddNewCharacter();
                }
                
                yield return new LoginInfo(_login, new string(_passwordAttempt.ToArray()));
            }
        }

        // Добавление в конец списка первого символа алфавита и сброс всего пароля
        private void AddNewCharacter()
        {
            _passwordAttempt.Add(_alphabet[0]);

            for (int i = 0; i < _passwordAttempt.Count; i++)
            {
                _passwordAttempt[i] = _alphabet[0];
            }
        }
        
        //В текущем индексе подставляется каждый символ алфавита 
        private void UpdatePasswordAttempt(int index)
        {
            if (index != _passwordAttempt.Count - 1)
            {
                for (int i = index + 1; i < _passwordAttempt.Count; i++)
                {
                    _passwordAttempt[i] = _alphabet[0];
                }
            }

            _passwordAttempt[index]++;
        }

        private int GetUpdateIndex(int index)
        {
            if (_passwordAttempt[index] == _alphabet[^1])
            {
                // Порорзрядное смещение налево.
                if (index != 0)
                {
                    index = GetUpdateIndex(index - 1);
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