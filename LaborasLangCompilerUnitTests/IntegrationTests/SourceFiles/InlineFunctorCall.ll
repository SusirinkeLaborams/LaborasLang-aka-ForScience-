use System;
entry auto Main = void()
{
	getFoo()();
};

auto foo = void()
{
	Console.Write("It Works!");
};

public auto getFoo = void()()
{
	return foo;
};