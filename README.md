# Inflection

## Tl;dr
This library makes using immutable object graphs much more pleasant to use.

## Abstract
Extends .Net's property system to inflect immutable semantics for immutable datatypes.

## Motivation
Imagine you have an immutable data structure composed of immutable objects in C#.
There's a few different conventions for how to achieve such a structure and most of them produce tons of boilerplate.
Where a simple mutable type takes a few lines of code with auto properties, the immutable variant has to add bloat at nearly every layer.

Have a look at the following example of a reference immutable Type `Employee`:
```C#
public class Employee {
    public Employee(string firstname, string lastname, double salary) {
        this.Firstname = firstname;
        this.Lastname = lastname;
        this.Salary = salary;
    }
    
    public string Firstname { get; private set; }
    
    public string Lastname { get; private set; }
    
    public double Salary { get; private set; }
    
    public Employee WithFirstname(string value) {
        return value == this.Firstname ? this : new Employee(value, this.Lastname, this.Salary);
    }
    
    public Employee WithLastname(string value) {
        return value == this.Lastname ? this : new Employee(this.Firstname, value);
    }
    
    public Employee WithYealrySalary(double value) {
        return value == this.Salary ? this : new Employee(this.Firstname, this.Lastname, value);
    }
}
```

Due to the `With*()` methods we can write `bill.WithSalary(15)` instead of `new Employee(bill.Firstname, bill.Lastname, 15)` which is nice.

But still, this turns into a hassle with object hierarchies.
```C#
// Mission: Return a new company in which Bill's salary has increased by x percent
public Company IncreaseSalary(Company company, double x) {
    // Find bill
    var depts = company.Departments;
    Department billDept = null;
    Employee bill = null;
    
    foreach(var dept in depts) {
        bill = dept.Employees.FirstOrDefault(x => x.Firstname == "Bill");
        if(bill != null) {
            billDept = dept;
            break;
        }
    }
    
    // Bill not found
    if(bill == null) {
        return company;
    }
    
    // Increase his salary
    var salary = bill.Salary * (1.0 + x);    
    var bill2 = bill.WithSalary(salary);
    
    // Update the company..
    var employees2 = dept.Employees.Replace(bill, bill2);
    var dept2 = billDept.WithEmployees(employees2);
    var departments2 = depts.Replace(billDept, dept2);
    var company2 = company.WithDepartments(departments2);
    
    return company2;
}
```

That's a lot of boilerplate for what would have been a one-liner in mutable code.

With Inflection you can scrap that boilerplate:
```C#
private static TypeGraph<Company> companyGraph = TypeGraph.Create<Company>(...);

// Mission: Return a new company in which Bill's salary has increased by x percent
public Company IncreaseSalary(Company company, double x) {
    // Find bill
    var billDescendant = companyGraph.GetDescendants<Employee>()
                                     .FirstOrDefault(x => x.Get(company).Firstname == "Bill");
    
    // Not found
    if(billDescendant == null) {
        return company;
    }
    
    // Increase bill's salary and update the company
    var company2 = billDescendant.GetChild(x => x.Salary)
                                 .Update(company, salary => salary * (1.0 + x));
    
    return company2;
}
```

## Inflection
This is a library for library writers.
I didn't even know about them when I started, but the library has somewhat turned out like Haskell's SYB.

## TBD
