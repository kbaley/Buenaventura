namespace Buenaventura.Domain;

public static class Irr
{
    public delegate double Fx(double x);

    static Fx ComposeFunctions(Fx f1, Fx f2)
    {
        return (double x) => f1(x) + f2(x);
    }

    static Fx F_xirr(double p, double dt, double dt0)
    {
        return (double x) => p * Math.Pow((1.0 + x), ((dt0 - dt) / 365.0));
    }

    static Fx Df_xirr(double p, double dt, double dt0)
    {
        return (double x) => (1.0 / 365.0) * (dt0 - dt) * p * Math.Pow((x + 1.0), (((dt0 - dt) / 365.0) - 1.0));
    }

    static Fx Total_f_xirr(double[] payments, double[] days)
    {
        Fx resf = (double x) => 0.0;

        for (var i = 0; i < payments.Length; i++)
        {
            resf = ComposeFunctions(resf, F_xirr(payments[i], days[i], days[0]));
        }

        return resf;
    }

    static Fx Total_df_xirr(double[] payments, double[] days)
    {
        Fx resf = (double x) => 0.0;

        for (var i = 0; i < payments.Length; i++)
        {
            resf = ComposeFunctions(resf, Df_xirr(payments[i], days[i], days[0]));
        }

        return resf;
    }

    public static double CalculateIrr(double[] cashflow, double[] days)
    {
        var irr = NewtonsMethod(0.1, Total_f_xirr(cashflow, days), Total_df_xirr(cashflow, days));
        if (double.IsNaN(irr))
        {
            // try a negative guess
            irr = NewtonsMethod(-0.5, Total_f_xirr(cashflow, days), Total_df_xirr(cashflow, days));
        }

        return irr;
    }

    static double NewtonsMethod(double guess, Fx f, Fx df)
    {
        var x0 = guess;
        double x1;
        var err = 1e+100;
        var tolerance = 0.001;

        while (err > tolerance)
        {
            x1 = x0 - f(x0) / df(x0);
            err = Math.Abs(x1 - x0);
            x0 = x1;
        }

        return x0;
    }
}