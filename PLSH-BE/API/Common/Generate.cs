namespace API.Common;

public abstract class Generate
{
  public static int Generate6DigitNumber()
  {
    var random = new Random();
    return random.Next(100000000, 1000000000); 
  }
}