auto Main = void()
{
	auto quit = false;
	System.Console.WriteLine("Enter 0 at any time to quit.");

	while (!quit)
	{
		int value;
		auto line = System.Console.ReadLine();

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

	auto isPrime = true;
	int divisor = System.Convert.ToInt32(System.Math.Sqrt(value) + 1);

	while (--divisor >= 2)
	{
		if (value % divisor == 0)
		{
			return false;
		}
	}

	return true;
};