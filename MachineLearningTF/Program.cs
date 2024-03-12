#region exemples recherche dichotomique - calcul racine
//using Plotly.NET.CSharp;

//int randomNumber = new Random().Next(1, 101);


////for (int i = 1; i <= 100; i++)
////{
////    if(i == randomNumber)
////    {
////        Console.WriteLine("Le nombre est " + i);
////    }
////}


////int value = new Random().Next(1, 101);
////while (value != randomNumber)
////{
////    value = new Random().Next(1, 101);
////}

////Console.WriteLine(value);

//// recherche dichotomique (o(log(n)))
////int max = 100;
////int min = 1;
////int a = (min + max) / 2;

////while (a != randomNumber)
////{
////    if(a > randomNumber)
////    {
////        max = a - 1;
////        a = (min + max) / 2;
////    }
////    else
////    {
////        min = a + 1;
////        a = (min + max) / 2;
////    }
////}

////(double, int) Sqrt(double x) // x >= 1
////{
////    int c = 0;
////    double a = new Random().NextDouble() * x;
////    while(Math.Abs(a * a - x) > 0.00000001)
////    {
////        c++;
////        a = new Random().NextDouble() * x;
////    }
////    return (a, c);
////}
////Console.WriteLine(Sqrt(2));


//(double, int) Sqrt(double x)
//{
//    double a = 1;
//    double i = 0.1;
//    int c = 0;
//    while (Math.Abs(a * a - x) > 0.00000001)
//    {
//        c++;
//        // recalculer a sur base de a et de i
//        a += i;
//        if(a * a > x)
//        {
//            a -= i;
//            i /= 10;
//        }
//        if(c % 1000 == 0)
//        {
//            Console.WriteLine(c);
//        }
//    }
//    return (a, c);
//}

////(double a, int c) = Sqrt(9999999999999);
////Console.WriteLine((a, c));

////double ytrue = Math.Sqrt(8547);
////List<(int, double)> L = new List<(int, double)>(); 

////(double, int) SqrtOpti(double x)
////{
////    double a = 1;
////    int c = 0;
////    while (c < 100)
////    {
////        L.Add((c, Math.Abs(ytrue - a)));
////        c++;
////        a = (a * a + x) / (2 * a);
////    }
////    return (a, c);
////}

////Console.WriteLine(SqrtOpti(8547));

////Chart.Spline<int, double, string>(
////    x: L.Select(l => l.Item1),
////    y: L.Select(l => l.Item2),
////    Name: "Precision de l'algorithme",
////    // mode: Plotly.NET.StyleParam.Mode.Markers,
////    MarkerColor: Plotly.NET.Color.fromKeyword(Plotly.NET.ColorKeyword.Red)
////).Show(); 
#endregion

using MachineLearningTF.Models;
using CsvHelper;
using Plotly.NET.CSharp;
using System.Globalization;
using NumSharp;
using ShellProgressBar;

List<Plant> plants = Read<Plant>("Data/blobs.csv");
plants = plants.Select(p => new Plant
{
    Length = (p.Length - plants.Min(p => p.Length)) / (plants.Max(p => p.Length) -plants.Min(p => p.Length)),
    Width = (p.Width - plants.Min(p => p.Width)) / (plants.Max(p => p.Width) - plants.Min(p => p.Width)),
    Toxic = p.Toxic,
}).ToList();

// données d'entrainements (csv) - initialisation des matrices
double[,] xData = new double[100, 2];
double[,] yData = new double[100, 1];
for(int i = 0; i < 100; i++)
{
    xData[i, 0] = plants[i].Width; 
    xData[i, 1] = plants[i].Length;
    yData[i, 0] = plants[i].Toxic;
}
// 100 X 2 => paramètres
NDArray X = new NDArray(xData);
// 100 X 1 => valeurs
NDArray y = new NDArray(yData);

// (facultatif) permet de stocker l'amélioration de la précision
List<(int, double)> L = new List<(int,double)>();

// (facultatif) pour afficher une barre de progression
// using ProgressBar bar = new ProgressBar(1000, "Work in progress");

(NDArray W, NDArray b) = Init(X);
// données approximatives (aléatoires)
// 1 X 2 // 1 X 1
// w1x1 + w2x2 + w3x3 + ... + b

for (int i = 0; i < 1000; i++)
{
    // (facultatif) augmenter la progession à chaque itération
    //bar.Tick();
    
    // calcul de la vraissemblance des données
    NDArray A = Model(X, W, b);

    // (facultatif) pour garder en mémoire la précision
    L.Add((i, Logloss(A, y)));

    // Calcul des gradients/dérivées pour mettre à jour les valeurs (W et b)
    (NDArray dw, NDArray db) = Gradients(A, X, y);

    // mise à jour de W et b
    (W, b) = Update(W, b, dw, db, 0.1);
}


//Console.WriteLine("W apres 1000 iterations: " + W);
//Console.WriteLine("b apres 1000 iterations: " + b);

// afficher l'évolution de la précision
Chart.Spline<int, double, string>(
    x: L.Select(d => d.Item1),
    y: L.Select(d => d.Item2),
    Name: "Log Loss"
).Show();

ShowPlantsChart(
    plants.Where(p => p.Toxic == 0),
    plants.Where(p => p.Toxic == 1)
);

// double percent = Predict(0, 2, W, b);

//Console.SetCursorPosition(0, 10);
//Console.WriteLine(percent);

void ShowPlantsChart(IEnumerable<Plant> nToxics, IEnumerable<Plant> toxics)
{
    Chart.Combine([
        // plantes non toxiques
        Chart.Scatter<double, double, string>(
            x: nToxics.Select(p => p.Width), // largeurs
            y: nToxics.Select(p => p.Length), // longueurs
            mode: Plotly.NET.StyleParam.Mode.Markers,
            MarkerColor: Plotly.NET.Color.fromKeyword(
                Plotly.NET.ColorKeyword.Green
            ),
            Name: "Plantes non toxiques"
        ),
        // plantes toxiques
        Chart.Scatter<double, double, string>(
            x: toxics.Select(p => p.Width), // largeurs
            y: toxics.Select(p => p.Length), // longueurs

            mode: Plotly.NET.StyleParam.Mode.Markers,
            MarkerColor: Plotly.NET.Color.fromKeyword(
                Plotly.NET.ColorKeyword.Red
            ),
            Name: "Plantes toxiques"
        ),
        Chart.Line<int, double, string>(
            x: [-1, 1],
            y: [
                (- W[0].GetDouble() * -1 - b.GetDouble()) / W[1].GetDouble(),
                (- W[0].GetDouble() * 1 - b.GetDouble()) / W[1].GetDouble(),
            ]
        )
        // w1x1 + w2x2 + b = 0
        // x2 = (- w1x1 -b)/w2
]).Show();
}

(NDArray, NDArray) Init(NDArray X)
{
    double[,] wData = new double[X.Shape[1], 1];
    for(int i = 0; i < X.Shape[1]; i++)
    {
        wData[i, 0] = np.random.randn();
    }
    NDArray W = new NDArray(wData);
    NDArray b = new NDArray(
        new double[,] { { np.random.randn() } }
    );
    return (W, b);
}

NDArray Model(NDArray X, NDArray W, NDArray b)
{
    // e^ => exponentielle
    // Z = X * W + b
    NDArray Z = np.dot(X, W) + b;
    // A = 1 / (1 + e^(-Z))
    NDArray A = 1 / (1 + np.exp(-1 * Z));
    return A;
}

double Logloss(NDArray A, NDArray y)
{
    double epsilon = 0.0000000000001;
    // L = (-1 / n) Σ y *log(A) + (y - 1) * log(1 - A)
    NDArray L = (-1d / A.size) * (y * np.log(A + epsilon) + (y - 1) * np.log(1 - A + epsilon)).ToArray<double>().Sum();
    return L;
}

(NDArray, NDArray) Gradients(NDArray A, NDArray X, NDArray y)
{
    // X^T: Transposée de X
    // dw = (1 / n) * X^T * (A - y)
    // db = (1 / n) * Σ A - y
    NDArray dw = (1d / A.size) * np.dot(X.T, A - y);
    NDArray db = (1d / A.size) * (A - y).ToArray<double>().Sum();
    return (dw, db);
}

(NDArray , NDArray) Update(NDArray W, NDArray b, NDArray dw, NDArray db, double a)
{
    W = W - a * dw;
    b = b - a * db;
    return (W, b);
}

double Predict(double width, double length, NDArray W, NDArray b)
{
    // 1 X 2
    double[,] wl = new double[,] { { width , length } };
    NDArray data = new NDArray(wl);
    return Model(data, W, b).GetDouble() * 100;
}

List<T> Read<T>(string file)
{
    using StreamReader reader = new StreamReader(file);

    using CsvReader csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

    return csvReader.GetRecords<T>().ToList();
}