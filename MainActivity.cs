/* Author: Reece Pieri
 * Date: 23/02/2020
 * Name: EZ Currency Converter
 * Version: 1.1.0
 * 
 * v1.1.0, 23/02/2020 - Added button and functionality to switch currencies around and display new values.
 */

using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace EZ_Currency_Converter
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private static string convertFrom;
        private static string convertTo;
        private static JObject parsed;
        private static string selection1;
        private static string selection2;
        private static double input = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Create spinner item selection events.
            var spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner1.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Spinner1_ItemSelected);
            var spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);
            spinner2.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Spinner2_ItemSelected);

            // Link currency array to spinner adapter.
            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.currencies, Android.Resource.Layout.SimpleSpinnerItem);

            // Link adapter to spinners.
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner1.Adapter = adapter;
            spinner2.Adapter = adapter;

            // Update results based on amount entered. Updates on clicks.
            var editText1 = FindViewById<EditText>(Resource.Id.editText1);
            editText1.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                if (editText1.Text.Length > 0)
                {
                    input = Convert.ToDouble(e.Text.ToString());
                    DisplayResults();
                } else
                {
                    input = 1;
                    DisplayDefaults();
                }
            };

            // Button function to switch currencies.
            Button switchbtn = FindViewById<Button>(Resource.Id.switchbtn);

            switchbtn.Click += (o, e) =>
            {
                SwitchCurrencies();
            };
        }

        // Executes on spinner1 selection.
        private void Spinner1_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner1 = (Spinner)sender;
            selection1 = spinner1.GetItemAtPosition(e.Position).ToString();
            if (!selection1.Contains("Select"))
            {
                convertFrom = selection1.Substring(0, 3);
                if (selection2.Contains("Select"))
                {
                    Thread thread = new Thread (new ThreadStart(RequestJson)); // Threading allows sharp response times on initial json request.
                    thread.Start();
                } else
                {
                    RequestJson();
                }

                if (!selection2.Contains("Select") || selection1 != selection2)
                {
                    if (input == 1)
                    {
                        DisplayDefaults();
                    } else
                    {
                        DisplayResults();
                    }
                }
            }
        }

        // Executes on spinner2 selection.
        private void Spinner2_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner2 = (Spinner)sender;
            selection2 = spinner2.GetItemAtPosition(e.Position).ToString();
            if (!selection2.Contains("Select"))
            {
                convertTo = selection2.Substring(0, 3);
                if (!selection1.Contains("Select") || selection1 != selection2)
                {
                    if (input == 1)
                    {
                        DisplayDefaults();
                    }
                    else
                    {
                        DisplayResults();
                    }
                }
            }
        }

        // Method to switch currencies.
        private void SwitchCurrencies()
        {
            int spinner1Current;
            int spinner2Current;

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.currencies, Android.Resource.Layout.SimpleSpinnerItem);
            var spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            var spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);

            spinner1Current = adapter.GetPosition(selection1);
            spinner2Current = adapter.GetPosition(selection2);

            spinner1.SetSelection(spinner2Current);
            spinner2.SetSelection(spinner1Current);
        }

        // Method to request json from API and convert to string.
        private void RequestJson()
        {
            string url = "https://api.exchangerate-api.com/v4/latest/" + convertFrom;
            WebRequest requestObjGet = WebRequest.Create(url);
            HttpWebResponse responseObjGet;
            responseObjGet = (HttpWebResponse)requestObjGet.GetResponse();

            string jsonstr;
            using Stream stream = responseObjGet.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            jsonstr = sr.ReadToEnd();
            sr.Close();

            parsed = JObject.Parse(jsonstr);
        }

        // Method to display results based on amount that the user inputs.
        private void DisplayResults()
        {
            TextView textView3 = FindViewById<TextView>(Resource.Id.textView3);
            TextView textView4 = FindViewById<TextView>(Resource.Id.textView4);
            textView3.Text = convertFrom + ": " + input;
            textView4.Text = convertTo + ": " + Math.Round((double)parsed["rates"][convertTo] * input, 4);
        }

        // Method to display default figures, convertFrom currency = 1.
        private void DisplayDefaults()
        {
            TextView textView3 = FindViewById<TextView>(Resource.Id.textView3);
            TextView textView4 = FindViewById<TextView>(Resource.Id.textView4);
            if (!selection1.Contains("Select") && !selection2.Contains("Select"))
            {
                if (selection1 != selection2)
                {
                    textView3.Text = convertFrom + ": " + input;
                    textView4.Text = convertTo + ": " + Math.Round((double)parsed["rates"][convertTo], 4);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}