using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Authenticator
{
    public class CodeGenerator
    {
        #region PrivateVars

        private static string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private int pinModulo;
        private HMAC mac;

        #endregion

        #region PublicProperties

        public static int pinCodeLength { get; set; }
        public static int intervalLength { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CodeGenerator()
        {
            pinCodeLength = 6;
            intervalLength = 30;
            pinModulo = (int)Math.Pow(10, pinCodeLength);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="PinCodeLength">Number of digits in desired Pin Code</param>
        /// <param name="IntervalLength">Number of seconds to base Pin Code on</param>
        public CodeGenerator(int PinCodeLength, int IntervalLength)
        {
            pinCodeLength = PinCodeLength;
            intervalLength = IntervalLength;
            pinModulo = (int)Math.Pow(10, pinCodeLength);
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Gets number of trailing zeros of a given integer
        /// </summary>
        /// <param name="num">Integer to find trailing zeros in</param>
        /// <returns>Number of trailing zeros</returns>
        private int numberOfTrailingZeros(int num)
        {
            string binValue = Convert.ToString(num, 2);
            if (Convert.ToInt32(binValue) == 0)
                return 1;
            int i = 0;
            while (Convert.ToInt32(binValue) % 10 == 0)
            {
                int tempNum = Convert.ToInt32(binValue);
                tempNum = tempNum / 10;
                binValue = tempNum.ToString();
                i++;
            }
            return i;
        }

        /// <summary>
        /// Converts a Base32 string to a compatible byte array
        /// </summary>
        /// <param name="encoded">Base32 String to decode</param>
        /// <returns>Byte array containing data</returns>
        private byte[] FromBase32String(string encoded)
        {
            // Remove whitespace and separators
            encoded = encoded.Trim().Replace(" ", "");
            // Canonicalize to all upper case
            encoded = encoded.ToUpper();
            char[] DIGITS = ValidChars.ToCharArray();
            int MASK = DIGITS.Length - 1;
            int SHIFT = numberOfTrailingZeros(DIGITS.Length);

            if (encoded.Length == 0)
            {
                return new byte[0];
            }
            int encodedLength = encoded.Length;
            int outLength = encodedLength * SHIFT / 8;
            byte[] result = new byte[outLength];
            int buffer = 0;
            int next = 0;
            int bitsLeft = 0;
            Dictionary<char, int> CHAR_MAP = new Dictionary<char, int>();
            for (int i = 0; i < DIGITS.Length; i++)
            {
                CHAR_MAP.Add(DIGITS[i], i);
            }

            foreach (char c in encoded.ToCharArray())
            {

                buffer <<= SHIFT;
                buffer |= CHAR_MAP[c] & MASK;
                bitsLeft += SHIFT;
                if (bitsLeft >= 8)
                {
                    result[next++] = (byte)(buffer >> (bitsLeft - 8));
                    bitsLeft -= 8;
                }
            }
            // We'll ignore leftover bits for now. 
            // 
            // if (next != outLength || bitsLeft >= SHIFT) {
            //  throw new DecodingException("Bits left: " + bitsLeft);
            // }
            return result;
        }

        /// <summary>
        /// Pads the output string with leading zeroes just in case the result is less than the length of desired digits
        /// </summary>
        /// <param name="value">Value to pad</param>
        /// <returns>Padded Result</returns>
        private String padOutput(int value)
        {
            String result = value.ToString();
            for (int i = result.Length; i < pinCodeLength; i++)
            {
                result = "0" + result;
            }
            return result;
        }

        /// <summary>
        /// Generates a PIN of desired length when given a challenge (counter)
        /// </summary>
        /// <param name="challenge">Counter to calculate hash</param>
        /// <returns>Desired length PIN</returns>
        private String generateResponseCode(long challenge)
        {
            byte[] value = BitConverter.GetBytes(challenge);
            Array.Reverse(value); //reverses the challenge array due to differences in c# vs java
            mac.ComputeHash(value);
            byte[] hash = mac.Hash;
            int offset = hash[hash.Length - 1] & 0xF;
            byte[] SelectedFourBytes = new byte[4];
            //selected bytes are actually reversed due to c# again, thus the weird stuff here
            SelectedFourBytes[0] = hash[offset];
            SelectedFourBytes[1] = hash[offset + 1];
            SelectedFourBytes[2] = hash[offset + 2];
            SelectedFourBytes[3] = hash[offset + 3];
            Array.Reverse(SelectedFourBytes);
            int finalInt = BitConverter.ToInt32(SelectedFourBytes, 0);
            int truncatedHash = finalInt & 0x7FFFFFFF; //remove the most significant bit for interoperability as per HMAC standards
            int pinValue = truncatedHash % pinModulo; //generate 10^d digits where d is the number of digits
            return padOutput(pinValue);
        }

        /// <summary>
        /// Gets current interval number since Unix Epoch based on given interval length
        /// </summary>
        /// <returns>Current interval number</returns>
        private long getCurrentInterval()
        {
            TimeSpan TS = (App.Current as App).CurrentTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);            
            long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
            long currentInterval = currentTimeSeconds / intervalLength;
            return currentInterval;
        }       

        #endregion

        #region PublicMethods

        /// <summary>
        /// Generates PIN code based on given Base32 secret code, interval length, and desired PIN code length
        /// </summary>
        /// <param name="secret">Base32 Secret Code</param>
        /// <returns>PIN code with desired number of digits</returns>
        public string computePin(string secret)
        {
            string strToReturn = "";
                try
                {
                    byte[] keyBytes = FromBase32String(secret);
                    mac = new HMACSHA1(keyBytes);
                    mac.Initialize();
                    strToReturn = generateResponseCode(getCurrentInterval());
                }
                catch (Exception e)
                {
                    return e.Message;
                }

            return strToReturn;
        }

        /// <summary>
        /// Gets number of seconds left in the current interval
        /// </summary>
        /// <returns></returns>
        public static int numberSecondsLeft()
        {
            TimeSpan TS = (App.Current as App).CurrentTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
            int secondsElapsed = (int)currentTimeSeconds % intervalLength;
            int secondsLeft = intervalLength - secondsElapsed;
            return secondsLeft;
        }

        #endregion
    }
}
