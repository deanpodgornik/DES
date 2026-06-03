using System.IO.Ports;
using System.Text;

namespace ScreenAutoClicker;

/// <summary>
/// Vmesnik za Partner Tech CD-7220 VFD prikazovalnik cen (2×20 znakov, DSP-800 protokol).
/// Serijsko: 9600 baud, 8N1, brez pretočnega nadzora.
/// </summary>
class Cd7220Display : IDisposable
{
    private const int BaudRate     = 9600;
    private const int DisplayCols  = 20;

    // DSP-800 / CD5220 ukazi
    private static readonly byte[] CmdInit  = { 0x1B, 0x40 };       // ESC @ – inicializacija + brisanje
    private static readonly byte[] CmdClear = { 0x0C };             // FF  – počisti zaslon
    private static readonly byte[] CmdCrLf  = { 0x0D, 0x0A };      // CR LF – premik na vrstico 2

    private readonly SerialPort _port;
    private bool _disposed;

    public bool IsOpen => _port.IsOpen;

    public Cd7220Display(string portName)
    {
        _port = new SerialPort(portName, BaudRate, Parity.None, 8, StopBits.One)
        {
            Handshake    = Handshake.None,
            WriteTimeout = 1000,
            ReadTimeout  = 500,
        };
    }

    public void Open()
    {
        _port.Open();
        // Počisti zaslon ob odprtju
        _port.Write(CmdInit, 0, CmdInit.Length);
        Thread.Sleep(50);
        _port.Write(CmdClear, 0, CmdClear.Length);
        Thread.Sleep(50);
    }

    /// <summary>
    /// Zapiše dve vrednosti na prikazovalnik – vsako v svojo vrstico, poravnano desno.
    /// Vrne true ob uspehu, false ob napaki.
    /// </summary>
    public bool ShowValues(string value1, string value2)
    {
        if (!_port.IsOpen) return false;

        try
        {
            string line1 = Format(value1);
            string line2 = Format(value2);

            // Počisti + začni od vrha
            _port.Write(CmdClear, 0, CmdClear.Length);
            Thread.Sleep(30);

            // Vrstica 1
            _port.Write(Encoding.ASCII.GetBytes(line1), 0, DisplayCols);

            // Premik na vrstico 2
            //_port.Write(CmdCrLf, 0, CmdCrLf.Length);

            // Vrstica 2
            //_port.Write(Encoding.ASCII.GetBytes(line2), 0, DisplayCols);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CD7220] Napaka pri pisanju: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Počisti zaslon (zapiše presledke).
    /// </summary>
    public void Clear()
    {
        if (!_port.IsOpen) return;
        _port.Write(CmdClear, 0, CmdClear.Length);
    }

    // Desno poravna besedilo in ga natančno izreže/dopolni na 20 znakov.
    private static string Format(string value)
    {
        string v = value ?? "";
        if (v.Length > DisplayCols)
            v = v.Substring(v.Length - DisplayCols); // ohrani zadnjih 20 znakov
        return v.PadLeft(DisplayCols);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_port.IsOpen)
            {
                Clear();
                _port.Close();
            }
        }
        catch { }
        _port.Dispose();
    }
}
