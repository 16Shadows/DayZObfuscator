using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public class ParserErrorBase<MessageType>
	{
		public ParserErrorBase(LexerTokenBase erroneousToken, MessageType message)
		{
			ErroneousToken = erroneousToken ?? throw new ArgumentNullException(nameof(erroneousToken));
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public LexerTokenBase ErroneousToken { get; }
		public MessageType Message { get; }
	}
}
