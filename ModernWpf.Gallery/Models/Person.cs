namespace ModernWpf.Gallery.Models
{
    public sealed class Person
    {
        public Person(string firstName, string lastName, string company)
        {
            FirstName = firstName;
            LastName = lastName;
            Company = company;
        }

        public string FirstName { get; }

        public string LastName { get; }

        public string Name
        {
            get { return FirstName + " " + LastName; }
        }

        public string Company { get; }
    }
}
