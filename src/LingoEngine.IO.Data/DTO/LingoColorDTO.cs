namespace LingoEngine.IO.Data.DTO;

public struct LingoColorDTO
{
    public int Code { get; set; }
    public string Name { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public LingoColorDTO(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
        Name = "";
        Code = -1;
    }
    public LingoColorDTO(int code, string name, byte r, byte g, byte b, byte a)
    {
        Code = code;
        Name = name;
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
