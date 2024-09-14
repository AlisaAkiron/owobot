using System.Xml.Linq;

const string isoCurrencyCodeListUrl = "https://www.six-group.com/dam/download/financial-information/data-center/iso-currrency/lists/list-one.xml";

var staticDirectory = Path.Join(Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName, "static");
var targetFile = Path.Join(staticDirectory, "currencies.csv");

Console.WriteLine($"Target CSV File: {targetFile}");
Console.WriteLine("Press any key to continue...");
Console.ReadLine();

using var client = new HttpClient();
var xml = await client.GetStringAsync(isoCurrencyCodeListUrl);

/*
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<ISO_4217 Pblshd="2024-06-25">
	<CcyTbl>
		<CcyNtry>
			<CtryNm>AFGHANISTAN</CtryNm>
			<CcyNm>Afghani</CcyNm>
			<Ccy>AFN</Ccy>
			<CcyNbr>971</CcyNbr>
			<CcyMnrUnts>2</CcyMnrUnts>
		</CcyNtry>
		<CcyNtry>
			<CtryNm>ÅLAND ISLANDS</CtryNm>
			<CcyNm>Euro</CcyNm>
			<Ccy>EUR</Ccy>
			<CcyNbr>978</CcyNbr>
			<CcyMnrUnts>2</CcyMnrUnts>
		</CcyNtry>
    </CcyTbl>
</ISO_4217>
*/

var doc = XDocument.Parse(xml);

var currencies = doc.Descendants("CcyNtry")
    .Select(x => new
    {
        Name = x.Element("CcyNm")?.Value,
        Code = x.Element("Ccy")?.Value,
        MinorUnits = x.Element("CcyMnrUnts")?.Value
    })
    .Where(x => x.Name is not null && x.Code  is not null && x.MinorUnits is not null)
    .Where(x => int.TryParse(x.MinorUnits, out _))
    .DistinctBy(x => x.Code)
    .ToList();

Console.WriteLine($"Get {currencies.Count} currencies");

// write to CSV
await using var file = File.Open(targetFile, FileMode.Create);
await using var writer = new StreamWriter(file);
await writer.WriteLineAsync("Name,Code,Digits");

foreach (var currency in currencies)
{
    await writer.WriteLineAsync($"{currency.Name},{currency.Code},{currency.MinorUnits}");
}

Console.WriteLine("Done");
