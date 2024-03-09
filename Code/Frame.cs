public interface Frame<Self>
{
    Self Combine(Self other);
    Self Inverse();
}