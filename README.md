# LaborasLang
Procedural language with select functional features


##Introduction

```
entry auto greeter = void()
{
	System.Console.WriteLine("Hello, world!");
};
```
This hello world program, without having any groundbreaking changes, shows some key features of this language:
* function declaration looks just like variable declaration and assignment
* variable type will be determined by compiler if you ask us to do that by using "auto" instead of type
* we are selecting application entry point by marking it as "entry" instead of forcing you to use some special name for function


##Basic syntax
###Variables
```
int foo;
foo = 5;
```
```
auto foo = 5;
```

```
void() bar;
bar = void()
{
};
```
```
auto bar = void()
{
};
```


```
auto something_complex = int()()
{
	return int()
	{
		return 5;
	}
}
```

```
entry auto main = void()
{
}
```

###Conditional statements
```
if (true)
{

}
else
{

}
```
###Structures
###Loops
```
while (condition)
{

}
```
##Statements
###Operators
###Operator precedence
```
Parentheses
Period
PostfixOperator
PrefixOperator
MultiplicativeOperator (Remainder, Division, Multiplication)
AdditiveOperator (Minus, Plus)
ShiftOperator (LeftShift, RightShift)
RelationalOperator (LessOrEqual, MoreOrEqual, Less, More)
EqualityOperator (Equal, NotEqual)
BitwiseAnd
BitwiseXor
BitwiseOr
And
Or
Assignment operator
```
