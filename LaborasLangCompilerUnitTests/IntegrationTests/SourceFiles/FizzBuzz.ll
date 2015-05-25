use System;

entry auto main = void()
{
	auto fizz = bool(int n) { return n % 3 == 0; };
	auto buzz = bool(int n) { return n % 5 == 0; };

	for (int i = 1; i <= 16; i++)
	{
		if(fizz(i))
			Console.Write("Fizz");
		if(buzz(i))
			Console.Write("Buzz");
		if(!fizz(i) && !buzz(i))
			Console.Write(i);
		Console.WriteLine();
	}
};