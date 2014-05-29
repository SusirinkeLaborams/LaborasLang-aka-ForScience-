auto Main = void()
{
	auto quit = false;
	System.Console.WriteLine("Enter 0 at any time to quit.");

	while (!quit)
	{
		int value;
		string line = System.Console.ReadLine();

		if (System.Int32.TryParse(line, value))
		{
			if (value == 0)
			{
				quit = true;
			}
			else
			{
				if (IsPrime(value))
				{
					System.Console.WriteLine("{0} is a prime number", value);
				}
				else
				{
					System.Console.WriteLine("{0} is not a prime number", value);
				}
			}
		}
		else
		{
			System.Console.WriteLine("Entered value is not a number!");
		}
	}
};

auto IsPrime = bool(int value)
{
	if (value < 2)
	{
		return false;
	}
	
	auto isDivisor = bool(int value, int divisor) { return value % divisor == 0 && divisor < value; };

	auto isPrime = true;
	auto divisor = System.Math.Ceiling(System.Math.Sqrt(value)) + 1;

	while (--divisor >= 2)
	{
		if (isDivisor(value, divisor))
		{
			return false;
		}
	}

	return true;
};