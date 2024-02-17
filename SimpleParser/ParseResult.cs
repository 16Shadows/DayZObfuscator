using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public class ParseResult<ParserResult, ParserError>
	{
		public ParseResult(ParserResult result, bool success, IEnumerable<ParserError> errors)
		{
			Result = result;
			Success = success;
			Errors = errors ?? throw new ArgumentNullException(nameof(errors));
		}

		public ParserResult Result { get; }
	
		public bool Success { get; }

		public IEnumerable<ParserError> Errors { get; protected set; }

		public virtual ParseResult<T, ParserError> WithResultAs<T>(Func<ParserResult, T> converter)
		{
			ArgumentNullException.ThrowIfNull(converter);
			return new ParseResult<T, ParserError>(converter(Result), Success, Errors);
		}

		public ParseResult<ParserResult, ParserError> TransformErrors(Func<IEnumerable<ParserError>, IEnumerable<ParserError>> transformer)
		{
			ArgumentNullException.ThrowIfNull(transformer);
			Errors = transformer(Errors) ?? throw new ArgumentException($"{nameof(transformer)} returned null.");
			return this;
		}
	}
}
