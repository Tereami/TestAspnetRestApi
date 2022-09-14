namespace TestAspnetRestApi
{
    public class Person
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Age { get; set; } = -1;

        public Person(string name, int age)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Age = age;
        }

        public Person()
        {

        }
    }
}
