using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Encodings;
using System.Collections.Generic;
using System.Text.Unicode;
using System.Text.Encodings.Web;
namespace TestElva
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await FindPolygoneAndSaveShortenedIntoFileAsync();
        }
        static async Task FindPolygoneAndSaveShortenedIntoFileAsync()
        {
            InputParams parameters = ReadInputParamsFromConsole();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Referer", "TestApp");
            string uri = "https://nominatim.openstreetmap.org/search?q=" + parameters.ObjectToFindName + "&format=json&polygon_geojson=1";
            var StatusCodeAndResponseStringTuple = await SendGetRequestAsync(uri, headers);
            if (StatusCodeAndResponseStringTuple.Item1 == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine(StatusCodeAndResponseStringTuple.Item2);
                using (JsonDocument document = JsonDocument.Parse(StatusCodeAndResponseStringTuple.Item2))
                {
                    JsonElement root = document.RootElement;
                    List<string> list = new List<string>();
                    foreach (var a in root.EnumerateArray())
                    {
                        list.Add(ReduceTheNumberOfPoints(a.GetRawText(), parameters.PointPeriod));
                    }
                    string ShortenedPolygon = "[";
                    var arr = list.ToArray();
                    for (int i=0;i<arr.Length;i++)
                    {
                        if (i != 0)
                            ShortenedPolygon += ",";
                        ShortenedPolygon += arr[i];
                    }
                    ShortenedPolygon += "]";
                    File.WriteAllText(parameters.FileName, ShortenedPolygon);
                }
            }
            else Console.WriteLine("StatusCode:" + StatusCodeAndResponseStringTuple.Item1.ToString());

        }
        static string  ReduceTheNumberOfPoints(string jsonElement,int N)
        {

            var options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                //,WriteIndented = true
            };
            JsonDocument doc = JsonDocument.Parse(jsonElement);
            string geojsontype = doc.RootElement.GetProperty("geojson").GetProperty("type").ToString();
            switch (geojsontype)
            {
                case "Point":
                    break;
                case "LineString":
                    PolyData<GeoJsonLineString> polydataLineString = JsonSerializer.Deserialize<PolyData<GeoJsonLineString>>(jsonElement);
                    polydataLineString.geojson.ReduceTheNumberOfPoints(N);
                    jsonElement = JsonSerializer.Serialize(polydataLineString, options);
                    break;
                case "Polygon":
                    PolyData<GeoJsonPolygon> geoJsonPolygon = JsonSerializer.Deserialize<PolyData<GeoJsonPolygon>>(jsonElement);
                    geoJsonPolygon.geojson.ReduceTheNumberOfPoints(N);
                    jsonElement = JsonSerializer.Serialize(geoJsonPolygon, options);
                    break;
                case "MultiPolygon":
                    PolyData<GeoJsonMultiPolygon> geoJsonMultiPolygon = JsonSerializer.Deserialize<PolyData<GeoJsonMultiPolygon>>(jsonElement);
                    geoJsonMultiPolygon.geojson.ReduceTheNumberOfPoints(N);
                    jsonElement = JsonSerializer.Serialize(geoJsonMultiPolygon, options);
                    break;
                default:
                    Console.WriteLine("Unknown geojson type:" + geojsontype);
                    break;
            }
            return jsonElement;
        }
        static async Task test1()
        {
            InputParams parameters =  ReadInputParamsFromConsole();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Referer", "TestApp");
            string uri = "https://nominatim.openstreetmap.org/search?q=" + parameters.ObjectToFindName + "&format=json&polygon_geojson=1";
            var StatusCodeAndResponseStringTuple = await SendGetRequestAsync(uri, headers);
            if (StatusCodeAndResponseStringTuple.Item1 == System.Net.HttpStatusCode.OK)
            {
                object PolyDataObject=null;
                if (StatusCodeAndResponseStringTuple.Item2.Contains("\"type\":\"MultiPolygon"))
                {
                    Console.WriteLine("MultiPolygon");
                    Console.WriteLine(StatusCodeAndResponseStringTuple.Item2);
                    PolyDataObject = JsonSerializer.Deserialize<PolyData<GeoJsonMultiPolygon>[]>(StatusCodeAndResponseStringTuple.Item2);
                    foreach (PolyData<GeoJsonMultiPolygon> pd in (PolyData<GeoJsonMultiPolygon>[])PolyDataObject)
                        pd.geojson.ReduceTheNumberOfPoints(parameters.PointPeriod);

                }
                else if (StatusCodeAndResponseStringTuple.Item2.Contains("\"type\":\"LineString"))
                {
                    Console.WriteLine("LineString");
                    PolyDataObject = JsonSerializer.Deserialize<PolyData<GeoJsonLineString>[]>(StatusCodeAndResponseStringTuple.Item2);
                    foreach (PolyData<GeoJsonLineString> pd in (PolyData<GeoJsonLineString>[])PolyDataObject)
                        pd.geojson.ReduceTheNumberOfPoints(parameters.PointPeriod);

                }
                var options = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    //,WriteIndented = true
                };
                string ShortenedPolygon = JsonSerializer.Serialize(PolyDataObject, options);
                File.WriteAllText(parameters.FileName, ShortenedPolygon);
            }
            else Console.WriteLine("StatusCode:" + StatusCodeAndResponseStringTuple.Item1.ToString());
            
        }
        static InputParams ReadInputParamsFromConsole()
        {
            Console.WriteLine("\nВведите название искомого объекта");
            string ObjName = Console.ReadLine();
            Console.WriteLine("Введите требуемую периодичность точек:");
            int N=0;
            string period="";
            string filename = "";
            do
            {
                period = Console.ReadLine();
                bool CorrectValue = int.TryParse(period, out N);
            } while (N<=0);

            Console.WriteLine("\nВведите название файла для сохранения");
            filename = Console.ReadLine();
            if (!filename.Contains("."))
                filename += ".txt";
            return new InputParams(ObjName, N, filename);
        }
        static async ValueTask<(HttpStatusCode, string)> SendGetRequestAsync(string uri,Dictionary<string,string> headers)
        {
            HttpClient client = new HttpClient();
            foreach(var a in headers)
                client.DefaultRequestHeaders.Add(a.Key, a.Value);
            HttpResponseMessage response = await client.GetAsync(uri);
            string stringContent=null;
            if (response.IsSuccessStatusCode)
            {
                stringContent = await response.Content.ReadAsStringAsync();
            }
            return (response.StatusCode, stringContent);
        }
    }
    struct InputParams
    {
        public InputParams(string objectToFindName, int pointPeriod,string fileName)
        {
            ObjectToFindName = objectToFindName;
            PointPeriod = pointPeriod;
            FileName = fileName;
        }
        public string ObjectToFindName;
        public int PointPeriod;
        public string FileName;
    }
}
