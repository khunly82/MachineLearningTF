
using Plotly.NET.CSharp;

int randomNumber = new Random().Next(1, 101);


//for (int i = 1; i <= 100; i++)
//{
//    if(i == randomNumber)
//    {
//        Console.WriteLine("Le nombre est " + i);
//    }
//}


//int value = new Random().Next(1, 101);
//while (value != randomNumber)
//{
//    value = new Random().Next(1, 101);
//}

//Console.WriteLine(value);

// recherche dichotomique (o(log(n)))
//int max = 100;
//int min = 1;
//int a = (min + max) / 2;

//while (a != randomNumber)
//{
//    if(a > randomNumber)
//    {
//        max = a - 1;
//        a = (min + max) / 2;
//    }
//    else
//    {
//        min = a + 1;
//        a = (min + max) / 2;
//    }
//}

//(double, int) Sqrt(double x) // x >= 1
//{
//    int c = 0;
//    double a = new Random().NextDouble() * x;
//    while(Math.Abs(a * a - x) > 0.00000001)
//    {
//        c++;
//        a = new Random().NextDouble() * x;
//    }
//    return (a, c);
//}
//Console.WriteLine(Sqrt(2));


(double, int) Sqrt(double x)
{
    double a = 1;
    double i = 0.1;
    int c = 0;
    while (Math.Abs(a * a - x) > 0.00000001)
    {
        c++;
        // recalculer a sur base de a et de i
        a += i;
        if(a * a > x)
        {
            a -= i;
            i /= 10;
        }
        if(c % 1000 == 0)
        {
            Console.WriteLine(c);
        }
    }
    return (a, c);
}

//(double a, int c) = Sqrt(9999999999999);
//Console.WriteLine((a, c));

double ytrue = Math.Sqrt(8547);
List<(int, double)> L = new List<(int, double)>(); 

(double, int) SqrtOpti(double x)
{
    double a = 1;
    int c = 0;
    while (c < 100)
    {
        L.Add((c, Math.Abs(ytrue - a)));
        c++;
        a = (a * a + x) / (2 * a);
    }
    return (a, c);
}

Console.WriteLine(SqrtOpti(8547));

Chart.Spline<int, double, string>(
    x: L.Select(l => l.Item1),
    y: L.Select(l => l.Item2),
    Name: "Precision de l'algorithme",
    // mode: Plotly.NET.StyleParam.Mode.Markers,
    MarkerColor: Plotly.NET.Color.fromKeyword(Plotly.NET.ColorKeyword.Red)
).Show();