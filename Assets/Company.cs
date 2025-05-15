public class Company {
	private CompanyData data;

	public Company(CompanyData data) {
		this.data = data;
	}
}

public class CompanyData {
	public string name { get; set; }
	public string owner { get; set; }

	public CompanyData() {
	}

	public CompanyData(string name, string owner) {
		this.name = name;
		this.owner = owner;
	}
}