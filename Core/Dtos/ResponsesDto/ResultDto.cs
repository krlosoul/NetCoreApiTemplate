namespace Core.Dtos.ResponsesDto
{
	public class Result
	{
		public string? Message { get; set;}

		public Result() : base() { }

		protected Result(string message)
		{
			Message = message;
		}

		public static Result Success(string message) => new(message);
	}

	public class Result<T> : Result
	{
		public T? Data { get; set; }
		public int TotalRecords { get; set; }

		public Result() : base() { }

		private Result(T data, int totalRecords) : base(string.Empty)
		{
			Data = data;
			TotalRecords = totalRecords;
		}

		private Result(string message) : base(message) { }

		public static Result<T> Success(T value, int totalRecords) => new(value, totalRecords);
	}
}