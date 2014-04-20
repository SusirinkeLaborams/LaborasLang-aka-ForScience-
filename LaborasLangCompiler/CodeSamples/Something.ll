auto Func = void()
{
	System.Console.WriteLine("Func called");
};

auto Main = int()
{
	Func();
	System.Console.ReadKey();
	return 0;
};