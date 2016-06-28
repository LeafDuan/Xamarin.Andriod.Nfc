using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Nfc;
using Android.Nfc.Tech;

namespace Smartisan.Nfc
{
    [Activity(Label = "Smartisan.Nfc", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity//, Android.Nfc.NfcAdapter.ICreateNdefMessageCallback
    {
        NfcAdapter nfcAdapter;
        PendingIntent nfcPi;
        IntentFilter nfcFilter;
        Tag nfcTag;
        string newLine = System.Environment.NewLine;

        /*
         * learning from
         * http://blog.csdn.net/earbao/article/details/50961713
         */
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.ScanButton);
            button.Click += Scan; ;

            var writerButton = FindViewById<Button>(Resource.Id.WriteButton);
            writerButton.Click += Write;

            var label = FindViewById<TextView>(Resource.Id.ResultLabel);

            nfcAdapter = NfcAdapter.GetDefaultAdapter(ApplicationContext);
            if (nfcAdapter == null)
            {
                label.Text = "NFC is not available.";
                return;
            }

            if (!nfcAdapter.IsEnabled)
            {
                label.Text = "NFC is disabled.";
                return;
            }

            //nfcAdapter.SetNdefPushMessageCallback(this, this);
            var intent = new Intent(this, this.Class);
            intent.AddFlags(ActivityFlags.SingleTop);
            nfcPi = PendingIntent.GetActivity(this, 0, intent, 0);
            nfcFilter = new IntentFilter(NfcAdapter.ActionTechDiscovered);
            nfcFilter.AddCategory(Intent.CategoryDefault);
        }

        private void Scan(object sender, EventArgs e)
        {
            var label = FindViewById<TextView>(Resource.Id.DataLabel);

            try
            {
                if (nfcTag == null)
                {
                    label.Text = "nfc tag is null";
                    return;
                }

                var ndef = Ndef.Get(nfcTag);
                ndef.Connect();
                var data = Encoding.UTF8.GetString(ndef.NdefMessage.ToByteArray());
                ndef.Close();

                label.Text = $"Data:{newLine}{data}";
            }
            catch (Exception ex)
            {
                label.Text += $"{newLine} Exception: {newLine} {ex.Message} {newLine} {ex.StackTrace}";
            }
        }

        private void Write(object sender, EventArgs e)
        {
            var label = FindViewById<TextView>(Resource.Id.DataLabel);

            try
            {
                if (nfcTag == null)
                {
                    label.Text = "nfc tag is null";
                    return;
                }

                var data = DateTime.Now.ToString();
                var ndefRecord = new NdefRecord(NdefRecord.TnfMimeMedia,
                    null,
                    new byte[] { },
                    Encoding.UTF8.GetBytes(data));

                var ndef = Ndef.Get(nfcTag);
                ndef.Connect();
                ndef.WriteNdefMessage(new NdefMessage(ndefRecord));
                ndef.Close();

                label.Text = $"Data:{newLine}{data}";
            }
            catch (Exception ex)
            {
                label.Text += $"{newLine} Exception: {newLine} {ex.Message} {newLine} {ex.StackTrace}";
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            nfcAdapter.EnableForegroundDispatch(this, nfcPi, new IntentFilter[] { nfcFilter }, null);

            if (NfcAdapter.ActionTechDiscovered == Intent.Action)
            {
                ProcessIntent(Intent);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
            if (NfcAdapter.ActionTechDiscovered == intent.Action)
            {
                ProcessIntent(Intent);
            }
        }

        private void ProcessIntent(Intent intent)
        {
            var label = FindViewById<TextView>(Resource.Id.ResultLabel);

            try
            {
                nfcTag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

                label.Text =$"Id: 0x{Utils.ByteToHex(nfcTag.GetId())}";
                label.Text += $"{newLine}Techs: {newLine}";
                label.Text += string.Join(newLine, nfcTag.GetTechList());
            }
            catch (Exception ex)
            {
                label.Text += $"{newLine} Exception: {newLine} {ex.Message} {newLine} {ex.StackTrace}";
            }
        }
    }
}

