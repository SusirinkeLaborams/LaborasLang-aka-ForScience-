use System;

entry auto Main = void()
{
	auto quit = false;
	Console.WriteLine("Enter 0 at any time to quit.");

	while (!quit)
	{
		int value;
		string line = Console.ReadLine();

		if (int.TryParse(line, value))
		{
			if (value == 0)
			{
				quit = true;
			}
			else
			{
				if (IsPrime(value))
				{
					Console.WriteLine("{0} is a prime number", value);
				}
				else
				{
					Console.WriteLine("{0} is not a prime number", value);
				}
			}
		}
		else
		{
			Console.WriteLine("Entered value is not a number!");
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
	auto divisor = Convert.ToInt32(Math.Ceiling(Math.Sqrt(value)) + 1);

	while (--divisor >= 2)
	{
		if (isDivisor(value, divisor))
		{
			return false;
		}
	}

	return true;
};