using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuProj.Tasks.Utilities
{
    // Simple tokenizer for expandable properties. This does not handle Unicode text elements, so sentinel
    // characters with combining marks will cause invalid variable names. I don't imagine this will be
    // a significant problem.

    public class ExpansionTokenizer : IEnumerable<ExpansionToken>
    {
        // the pipe character is used because it is an invalid character in a filename (for Windows anyway),
        // used as an operator in bash, generally not allowed in a language identifier and it doesn't have to be 
        // escaped in XML.

        private const char Sentinel = '|';

        private string _inputText;

        public ExpansionTokenizer(string inputText)
        {
            _inputText = inputText ?? String.Empty;
        }

        public IEnumerator<ExpansionToken> GetEnumerator()
        {
            string inputText = _inputText;
            int inputLength = inputText.Length;
            int inputPosition = 0;

            while (inputPosition < inputLength)
            {
                // locate the next sentinel character in the input..
                int sentinelPosition = inputText.IndexOf(Sentinel, inputPosition);
                if (sentinelPosition < 0)
                {
                    // end of input..
                    yield return new ExpansionToken(ExpansionTokenType.Text, inputText.Substring(inputPosition));
                    break;
                }

                // all characters between inputPosition and sentinelPosition (including inputPosition) are
                // text characters..

                int skippedLength = sentinelPosition - inputPosition;
                if (skippedLength > 0)
                    yield return new ExpansionToken(ExpansionTokenType.Text, inputText.Substring(inputPosition, skippedLength));

                // locate the next sentinel after sentinelPosition, which will be the closing sentinel..
                int closingPosition = inputText.IndexOf(Sentinel, sentinelPosition + 1);
                if (closingPosition < 0)
                {
                    // no closing sentinel, remainder of input is text..
                    yield return new ExpansionToken(ExpansionTokenType.Text, inputText.Substring(sentinelPosition));
                    break;
                }

                // if the opening and closing sentinel are adjacent, then it is an escaped sentinel..
                if (closingPosition == sentinelPosition + 1)
                    yield return new ExpansionToken(ExpansionTokenType.Text, Sentinel.ToString());
                else 
                    // otherwise, everything between sentinelPosition and closingPosition is a variable token..
                    yield return new ExpansionToken(ExpansionTokenType.Variable, inputText.Substring(sentinelPosition + 1, closingPosition - sentinelPosition - 1));

                inputPosition = closingPosition + 1;
                continue;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
