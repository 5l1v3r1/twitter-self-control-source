/*
 * 
 * Twitter Self-Control Programı
 * 
 *  Kodlayan ve Tasarlayan
 *  Osman Yavuz
 *  Version: Beta v1.0
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Eklenenler
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading;


namespace Twitter_Self_Control
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Değişkenler
        string web1_HtmlKod = null;
        string web2_HtmlKod = null;
        string bagliHesap = null;
        bool cikisYap_Aktif = false;

        Thread tHashtagArama;

        bool hesapToplamaAktif = false;
        string hesapToplamaBaslamaZamani = null;
        Thread tTakipciToplama;

        Thread tTakipEtme;
        bool takipEtmeAktif = false;
        string takipEtmeBaslamaZamani = null;
        int takipEtme_Toplam = 0;
        int takipEtme_Kalan = 0;
        int takipEtme_TakipEdilen = 0;
        int takipEtme_Basarisiz = 0;

        bool twitterEngelAktif = false;
        string twitterEngelSure = null;

        Thread tTakipEttigimToplama;
        bool takipEttigimToplamaAktif = false;

        Thread tTakiptenCikarma;
        bool takiptenCikarmaAktif = false;
        string takiptenCikarmaBaslamaZamani = null;
        int takiptenCikarma_Toplam = 0;
        int takiptenCikarma_Kalan = 0;
        int takiptenCikarma_Cikarilan = 0;
        int takiptenCikarma_Basarisiz = 0;



        // Bilgi Mesajları fonksiyonu
        void bilgiMesajGonder(string mesaj, string durum = "")
        {
            // guncel zaman
            string guncelZaman = DateTime.Now.ToString();

            // İşlem penceresine mesajı gönder
            listBox_islemPenceresi.Items.Insert(0, "["+guncelZaman+"] " + mesaj);

            // label durum belirleme
            durum = durum.Trim().ToLower();
            if (durum == "hata")
            {
                label_Durum.ForeColor = Color.DarkRed;
            }
            else if (durum == "bilgi")
            {
                label_Durum.ForeColor = Color.DarkBlue;
            }
            else if (durum == "başarılı")
            {
                label_Durum.ForeColor = Color.DarkGreen;
            }
            else
            {
                label_Durum.ForeColor = Color.Black;
            }

            // label duruma mesajı gönder
            label_Durum.Text = mesaj;
        }

        // Veri Ayıklama Fonksiyonu
        public string veri;
        void veriAyiklama(string kaynakKod, string ilkVeri, int ilkVeriKS, string sonVeri)
        {
            try
            {
                string gelen = kaynakKod;
                int titleIndexBaslangici = gelen.IndexOf(ilkVeri) + ilkVeriKS;
                int titleIndexBitisi = gelen.Substring(titleIndexBaslangici).IndexOf(sonVeri);
                veri = gelen.Substring(titleIndexBaslangici, titleIndexBitisi);
            }
            catch //(Exception ex)
            {
                //MessageBox.Show("Hata: " + ex.Message, "Hata;", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #region FORM_LOAD
        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread Çalıştırma
            CheckForIllegalCrossThreadCalls = false;

            // Bilgi Mesajları
            bilgiMesajGonder("Program başlatıldı.", "bilgi");
        }
        #endregion

        #region Form_Shown
        private void Form1_Shown(object sender, EventArgs e)
        {
            // Bilgi Mesajları
            bilgiMesajGonder("Twitter'a bağlanılıyor. Lütfen Bekleyin..", "bilgi");

            // Twitter giriş formuna bağlanma
            webBrowser1.Navigate(textBox_Twitter_LoginURL.Text);

            // Dünya gündemi çekme - Sayfaya bağlan
            webBrowser2.Navigate(textBox_Twitter_GundemURL.Text);
        }
        #endregion

        #region Form_Closed
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Form_Closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Twitter Giriş Yapma İşlemleri

        #region [Giriş Yap Butonu]
        private void button_girisForm_GirisYap_Click(object sender, EventArgs e)
        {
            try
            {
                //Kullanıcı adı
                webBrowser1.Document.GetElementById("session[username_or_email]").InnerText = textBox_girisForm_Kadi.Text;
                
                //şifre
                webBrowser1.Document.GetElementById("session[password]").InnerText = textBox_girisForm_Sifre.Text;
                
                //Giriş Eventi
                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement el2 in elc2)
                {
                    if (el2.GetAttribute("value").Equals("Giriş yap"))
                    {
                        el2.InvokeMember("Click");
                    }
                }
            }
            catch
            {
                // Bilgi Mesajları
                bilgiMesajGonder("Twitter'a giriş yapılırken bir hata oluştur. Tekrar deneyin.", "hata");
            }
        }
        #endregion

        #region [Çıkış Yap Butonu]
        private void button_girisForm_CikisYap_Click(object sender, EventArgs e)
        {
            try
            {
                // Twitter giriş formuna bağlanma
                webBrowser1.Navigate(textBox_Twitter_LoginURL.Text);
                cikisYap_Aktif = true;
            }
            catch
            {
                // Bilgi Mesajları
                bilgiMesajGonder("Twitter'a giriş yapılırken bir hata oluştur. Tekrar deneyin.", "hata");
            }
        }
        #endregion

        #endregion

        #region Dünya Gündemi

        #region Dünya gündemi yenile butonu
        private void button_DunyaGundemi_Yenile_Click(object sender, EventArgs e)
        {
            // Sayfaya bağlan
            webBrowser2.Navigate(textBox_Twitter_GundemURL.Text);
        }
        #endregion

        #region seçili gündem
        private void listBox_DunyaGundemi_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_DunyaGundemi_Secili.Text = listBox_DunyaGundemi.Text;
        }
        #endregion

        #endregion

        #region Hashtag arama ile profil toplama

        #region Başlat butonu
        private void button_HashtagArama_Baslat_Click(object sender, EventArgs e)
        {
            if (textBox_HashtagArama_Kelime.TextLength > 0)
            {
                DialogResult soru = MessageBox.Show("Hashtag taraması başlatılsın mı?\n\nNOT: Bu işlemin uzunluğu o kelimenin potansiyeli ile doğru orantılıdır. Ayrıca internet hızı ve bilgisayar hızı bu işlemin süresine etki gösterebilir. Dilerseniz durdurabilir ve profil listesini kaydedebilirsiniz.", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (soru == DialogResult.Yes)
                {
                    // temizlik :)
                    listBox_HashtagArama_ProfilList.Items.Clear();
                    textBox_HashtagArama_ToplamHesap.Clear();

                    // Nesne pasifleştirme
                    button_HashtagArama_Baslat.Enabled = false;
                    textBox_HashtagArama_Kelime.Enabled = false;
                    button_HashtagArama_ListeyiKaydet.Enabled = false;

                    // nesne aktifleştirm
                    button_HashtagArama_Durdur.Enabled = true;

                    // Bilgi mesajları
                    bilgiMesajGonder("Hashtag ile hesap toplama işlemi başladı.", "bilgi");

                    // Tarama Başlat
                    tHashtagArama = new Thread(delegate()
                    {
                        hashtag_arama(textBox_HashtagArama_Kelime.Text);
                    });
                    tHashtagArama.Start();
                }
            }
            else
            {
                // Bilgi mesajları
                bilgiMesajGonder("Hashtag araması yapmak için kelime girmelisiniz.", "bilgi");
                MessageBox.Show("Hashtag araması yapmak için kelime girmelisiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // odaklanma
                textBox_HashtagArama_Kelime.Focus();
            }
        }
        #endregion

        #region Durdur butonu
        private void button_HashtagArama_Durdur_Click(object sender, EventArgs e)
        {
            DialogResult soru = MessageBox.Show("Hashtag taraması durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (soru == DialogResult.Yes)
            {
                // Nesne pasifleştirme
                button_HashtagArama_Durdur.Enabled = false;

                // nesne aktifleştirm
                button_HashtagArama_Baslat.Enabled = true;
                textBox_HashtagArama_Kelime.Enabled = true;
                button_HashtagArama_ListeyiKaydet.Enabled = true;

                // Bilgi mesajları
                bilgiMesajGonder("Hashtag taraması durduruldu. Toplam bulunan profil: " + listBox_HashtagArama_ProfilList.Items.Count.ToString(),"Bilgi");

                // Taramayı Durdur
                tHashtagArama.Abort();
            }
        }
        #endregion

        #region Listeyi kaydet butonu
        private void button_HashtagArama_ListeyiKaydet_Click(object sender, EventArgs e)
        {
            SaveFileDialog kaydetPencere = new SaveFileDialog();
            kaydetPencere.Title = "Kaydedilecek yeri seçin";
            kaydetPencere.Filter = "Txt dosyası |*.txt";

            if (kaydetPencere.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter kaydet = new StreamWriter(kaydetPencere.FileName);
                    foreach (var item in listBox_HashtagArama_ProfilList.Items)
                    {
                        kaydet.WriteLine(item);
                    }
                    kaydet.Close();

                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya başarıyla kaydedildi.", "başarılı");
                    MessageBox.Show("Dosya başarıyla kaydedildi.\nDosya yolu: " + kaydetPencere.FileName, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
                }
                catch
                {
                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya kaydedilemiyor. Tekrar deneyin..", "hata");
                    MessageBox.Show("Dosya kaydedilemiyor. Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
           
                }
            }
        }
        #endregion

        // Twitter Hashtag ile profil arama Fonksiyonu
        void hashtag_arama(string kelime, string cursor = "-1")
        {
            Uri url = new Uri("https://mobile.twitter.com/search?q=" + kelime + "&s=typd&next_cursor=" + cursor);
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string html = client.DownloadString(url);

            // hesapları çekip listeye ekleme
            try
            {
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);

                HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='username']");
                foreach (var veri2 in XPath)
                {
                    string hesap = veri2.InnerText.Remove(0, 1);
                    if (hesap.IndexOf(bagliHesap) != -1)
                    { }
                    else
                    {
                        if (listBox_HashtagArama_ProfilList.Items.Contains(hesap) == false)
                        {
                            // listbox'a ekle
                            listBox_HashtagArama_ProfilList.Items.Add(hesap.Trim());
                        }

                        // toplam hesap sayısı
                        textBox_HashtagArama_ToplamHesap.Text = listBox_HashtagArama_ProfilList.Items.Count.ToString();
                    }
                }
            }
            catch
            {
                /*
                // Nesne pasifleştirme
                button_HashtagArama_Durdur.Enabled = false;

                // nesne aktifleştirm
                button_HashtagArama_Baslat.Enabled = true;
                textBox_HashtagArama_Kelime.Enabled = true;
                button_HashtagArama_ListeyiKaydet.Enabled = true;

                // Bilgi mesajları
                bilgiMesajGonder("Hashtag aramasında sorun oldu! Tekrar deneyin..", "hata");
                MessageBox.Show("Hashtag aramasında sorun oldu! Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Taramayı Durdur
                //tHashtagArama.Abort();
              */
            }

            // cursor yakalama
            try
            {
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);

                HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='w-button-more']");
                foreach (var veri2 in XPath)
                {
                    // veri ayıklayarak cursor çekme
                    veriAyiklama(veri2.InnerHtml, "next_cursor=", 12, "\">");

                    // ayıklanan veri ile fonksiyon döndürme
                    // Tarama Başlat
                    tHashtagArama = new Thread(delegate()
                    {
                        hashtag_arama(kelime, veri);
                    });
                    tHashtagArama.Start();
                }
            }
            catch
            {
                // Nesne pasifleştirme
                button_HashtagArama_Durdur.Enabled = false;

                // nesne aktifleştirm
                button_HashtagArama_Baslat.Enabled = true;
                textBox_HashtagArama_Kelime.Enabled = true;
                button_HashtagArama_ListeyiKaydet.Enabled = true;

                // Bilgi mesajları
                bilgiMesajGonder("Hashtag taraması tamamlandı. Toplam bulunan profil: " + listBox_HashtagArama_ProfilList.Items.Count.ToString(), "Bilgi");
                MessageBox.Show("Hashtag taraması tamamlandı. Toplam bulunan profil: " + listBox_HashtagArama_ProfilList.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Taramayı Durdur
                tHashtagArama.Abort();
            }
        }

        #endregion

        #region webBrowser1 işlemleri [DocumentCompleted]
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                // webBrowser sayfa kaynak kod
                web1_HtmlKod = webBrowser1.Document.Body.InnerHtml.ToString();
                richTextBox1.Text = web1_HtmlKod;

                // webBrowser sayfa url adresi
                textBox1.Text = webBrowser1.Url.ToString();


                /* WEB İŞLEMLERİ */

                // Giriş yapılmamışsa
                if (web1_HtmlKod.IndexOf("<SPAN class=title>Giriş yap</SPAN>") != -1)
                {
                    // gizli nesneleri göster
                    groupBox_girisForm.Enabled = true;

                    // Bilgi Mesajları
                    bilgiMesajGonder("Twitter'a giriş yapabilirsiniz.", "bilgi");
                    label_girisForm_Durum.ForeColor = Color.DarkGreen;
                    label_girisForm_Durum.Text = "Giriş Yapabilirsiniz";
                }

                // Giriş yapılmışsa profile yönlendir
                else if (web1_HtmlKod.IndexOf("<TD class=me>") != -1)
                {
                    // Twitter profiline bağlanma
                    webBrowser1.Navigate(textBox_Twitter_ProfilURL.Text);
                }

                // Giriş yapılan hesabın bilgileri
                else if (web1_HtmlKod.IndexOf("<SPAN class=screen-name>") != -1)
                {
                    veriAyiklama(web1_HtmlKod, "<SPAN class=screen-name>", 24, "</SPAN>");
                    veri = veri.ToLower();

                    if (cikisYap_Aktif == false)
                    {
                        if (textBox_girisForm_Kadi.Text.ToLower().IndexOf(veri) != -1)
                        {
                            // nesne pasifleştirme
                            groupBox_girisForm.Enabled = false;

                            // Bağlı hesabı değişkene atama
                            bagliHesap = veri.ToLower();

                            // Bilgi Mesajları
                            bilgiMesajGonder(veri + " olarak giriş yapıldı.", "başarılı");
                            label_girisForm_Durum.ForeColor = Color.DarkGreen;
                            label_girisForm_Durum.Text = "Giriş Yapıldı";
                        }
                    }
                    else
                    {
                        //Çıkış yap Eventi
                        HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                        foreach (HtmlElement el2 in elc2)
                        {
                            if (el2.GetAttribute("value").Equals("Çıkış yap"))
                            {
                                el2.InvokeMember("Click");
                            }
                        }

                        // değişken atama
                        cikisYap_Aktif = false;

                        // Bilgi Mesajları
                        bilgiMesajGonder(bagliHesap + " twitter'dan çıkış yapıldı.", "bilgi");
                    }
                }

                // Toplu Takip Etme
                if (takipEtmeAktif == true)
                {

                    // Profil listesi tamamlanırsa
                    if (listBox_TakipEtme_ProfilListesi.Items.Count == listBox_TakipEtme_ProfilListesi.SelectedIndex + 1)
                    {
                        // Nesne aktifleştir
                        button_TakipEtme_ListeYükle.Enabled = true;
                        listBox_TakipEtme_ProfilListesi.Enabled = true;
                        button_TakipEtme_Baslat.Enabled = true;

                        // Nesne pasifleştir
                        button_TakipEtme_Durdur.Enabled = false;

                        // değişkene atama
                        takipEtmeAktif = false;
                        takipEtmeBaslamaZamani = null;

                        // Bilgi mesajları
                        label_TakipEtme_Durum.Text = "Tamamlandı";
                        bilgiMesajGonder("Toplu Takip Etme işlemi tamamlandı.", "Başarılı");
                        MessageBox.Show("Toplu Takip Etme işlemi tamamlandı.", "Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);

                        // Tarama Durdur
                        tTakipEtme.Abort();
                    }
                    else
                    {
                        // işlemdeki profil
                        string gecerliProfil = listBox_TakipEtme_ProfilListesi.Text;
                        label_TakipEtme_Durum.Text = gecerliProfil + " işlemde";

                        // Twitter Engellerse
                        if (web1_HtmlKod.IndexOf("Çok fazla denemede bulundun. Lütfen daha sonra tekrar dene.") != -1)
                        {
                            // Bilgi mesajları
                            bilgiMesajGonder("Çok fazla istek gönderildiği için Twitter engelledi. Biraz bekleyin.", "Bilgi");
                            listBox_TakipEtme_DurumPenceresi.Items.Insert(0, "Çok fazla istek gönderildiği için Twitter engelledi. Biraz bekleyin.");

                            // Değişken atama
                            DateTime ekle = DateTime.Now;
                            twitterEngelSure = ekle.AddSeconds(int.Parse(textBox_Twitter_Timeout.Text)).ToString();
                            twitterEngelAktif = true;
                        }
                        // Takip Ediliyor
                        else if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"Takip ediliyor\" name=commit>") != -1)
                        {
                            // Bilgi mesajları
                            bilgiMesajGonder(gecerliProfil + " profili takip edilmiş.", "Bilgi");
                            listBox_TakipEtme_DurumPenceresi.Items.Insert(0, gecerliProfil + " profili takip edilmiş.");

                            // Değişken atama
                            takipEtme_Kalan = takipEtme_Toplam - 1;
                            takipEtme_Toplam = takipEtme_Kalan;

                            // profil seçme
                            listBox_TakipEtme_ProfilListesi.SelectedIndex = listBox_TakipEtme_ProfilListesi.SelectedIndex + 1;

                            // Tarama Başlat
                            tTakipEtme = new Thread(delegate()
                            {
                                takipEtme(listBox_TakipEtme_ProfilListesi.Text);
                            });
                            tTakipEtme.Start();
                        }
                        // Takip Ediliyor
                        else if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"İsteği iptal et\" name=commit>") != -1)
                        {
                            // Bilgi mesajları
                            bilgiMesajGonder(gecerliProfil + " profile istek gönderilmiş.", "Bilgi");
                            listBox_TakipEtme_DurumPenceresi.Items.Insert(0, gecerliProfil + " profile istek gönderilmiş.");

                            // Değişken atama
                            takipEtme_Kalan = takipEtme_Toplam - 1;
                            takipEtme_Toplam = takipEtme_Kalan;

                            // profil seçme
                            listBox_TakipEtme_ProfilListesi.SelectedIndex = listBox_TakipEtme_ProfilListesi.SelectedIndex + 1;

                            // Tarama Başlat
                            tTakipEtme = new Thread(delegate()
                            {
                                takipEtme(listBox_TakipEtme_ProfilListesi.Text);
                            });
                            tTakipEtme.Start();
                        }
                        else
                        {

                            // Takip Et
                            if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"Takip et\" name=commit>") != -1)
                            {
                                // Bilgi mesajları
                                bilgiMesajGonder(gecerliProfil + " profili takip ediliyor.", "Bilgi");
                                listBox_TakipEtme_DurumPenceresi.Items.Insert(0, gecerliProfil + " profili takip ediliyor.");

                                // Değişken atama
                                takipEtme_Kalan = takipEtme_Toplam - 1;
                                takipEtme_Toplam = takipEtme_Kalan;
                                takipEtme_TakipEdilen = takipEtme_TakipEdilen + 1;

                                // Takip Et buton Eventi
                                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                                foreach (HtmlElement el2 in elc2)
                                {
                                    if (el2.GetAttribute("value").Equals("Takip et"))
                                    {
                                        el2.InvokeMember("Click");
                                    }
                                }

                            }
                            // İstek gönder
                            else if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"İstek gönder\" name=commit>") != -1)
                            {
                                // Bilgi mesajları
                                bilgiMesajGonder(gecerliProfil + " profile istek gönderildi.", "Bilgi");

                                // Değişken atama
                                takipEtme_Kalan = takipEtme_Toplam - 1;
                                takipEtme_Toplam = takipEtme_Kalan;
                                takipEtme_TakipEdilen = takipEtme_TakipEdilen + 1;

                                // Takip Et buton Eventi
                                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                                foreach (HtmlElement el2 in elc2)
                                {
                                    if (el2.GetAttribute("value").Equals("İstek gönder"))
                                    {
                                        el2.InvokeMember("Click");
                                    }
                                }
                            }
                        }


                        // Detaylar
                        textBox_TakipEtme_ToplamProfil.Text = listBox_TakipEtme_ProfilListesi.Items.Count.ToString();
                        textBox_TakipEtme_KalanProfil.Text = takipEtme_Kalan.ToString();
                        textBox_TakipEtme_TakipEdilen.Text = takipEtme_TakipEdilen.ToString();
                        textBox_TakipEtme_Basarisiz.Text = takipEtme_Basarisiz.ToString();
                    }

                }


                // Toplu Takipten Çıkarma
                if (takiptenCikarmaAktif == true)
                {

                    // Profil listesi tamamlanırsa
                    if (listBox_TakiptenCikarma_TakipListem.Items.Count == listBox_TakiptenCikarma_TakipListem.SelectedIndex + 1)
                    {
                        // Nesne aktifleştir
                        checkBox_TakiptenCikarma_TakipEttiklerimiCek.Enabled = true;
                        button_TakiptenCikarma_ListeYukle.Enabled = true;
                        button_TakiptenCikarma_ListeKaydet.Enabled = true;
                        button_TakiptenCikarma_Baslat.Enabled = true;

                        // Nesne pasifleştir
                        button_TakiptenCikarma_Durdur.Enabled = false;

                        // değişkene atama
                        takiptenCikarmaAktif = false;
                        takiptenCikarmaBaslamaZamani = null;

                        // Bilgi mesajları
                        label_TakiptenCikarma_Durum.Text = "Tamamlandı";
                        bilgiMesajGonder("Toplu Takipten Çıkarma işlemi tamamlandı.", "Başarılı");
                        MessageBox.Show("Toplu Takipten Çıkarma işlemi tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Tarama Durdur
                        tTakiptenCikarma.Abort();
                    }
                    else
                    {
                        // işlemdeki profil
                        string gecerliProfil = listBox_TakiptenCikarma_TakipListem.Text;
                        label_TakiptenCikarma_Durum.Text = gecerliProfil + " işlemde";

                        // Twitter Engellerse
                        if (web1_HtmlKod.IndexOf("Çok fazla denemede bulundun. Lütfen daha sonra tekrar dene.") != -1)
                        {
                            // Bilgi mesajları
                            bilgiMesajGonder("Çok fazla istek gönderildiği için Twitter engelledi. Biraz bekleyin.", "Bilgi");
                            listBox_TakipEtme_DurumPenceresi.Items.Insert(0, "Çok fazla istek gönderildiği için Twitter engelledi. Biraz bekleyin.");

                            // Değişken atama
                            DateTime ekle = DateTime.Now;
                            twitterEngelSure = ekle.AddSeconds(int.Parse(textBox_Twitter_Timeout.Text)).ToString();
                            twitterEngelAktif = true;
                        }



                        // Seni takip ediyor
                        if (web1_HtmlKod.IndexOf("<SPAN class=follows-you>Seni Takip Ediyor</SPAN>") != -1)
                        {
                            // Diğer profile atla

                            // Bilgi mesajları
                            bilgiMesajGonder(gecerliProfil + " profili seni takip ediyor.", "Bilgi");
                            listBox_TakiptenCikarma_İslemPenceresi.Items.Insert(0, gecerliProfil + " profili seni takip ediyor.");

                            // Değişken atama
                            takiptenCikarma_Kalan = takipEtme_Toplam - 1;
                            takiptenCikarma_Toplam = takiptenCikarma_Kalan;

                            // profil seçme
                            listBox_TakiptenCikarma_TakipListem.SelectedIndex = listBox_TakiptenCikarma_TakipListem.SelectedIndex + 1;

                            // Tarama Başlat
                            tTakiptenCikarma = new Thread(delegate()
                            {
                                takiptenCikarma(listBox_TakiptenCikarma_TakipListem.Text);
                            });
                            tTakiptenCikarma.Start();
                        }
                        else
                        {

                            if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"Takip ediliyor\" name=commit>") != -1)
                            {
                                //Takibi bırak Eventi
                                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                                foreach (HtmlElement el2 in elc2)
                                {
                                    if (el2.GetAttribute("value").Equals("Takip ediliyor"))
                                    {
                                        el2.InvokeMember("Click");
                                    }
                                }
                            }


                        // Takibi bırakma işlemi
                            else if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"Takibi bırak\" name=commit>") != -1)
                            {
                                //Takibi bırak Eventi
                                HtmlElementCollection elc2 = webBrowser1.Document.GetElementsByTagName("input");
                                foreach (HtmlElement el2 in elc2)
                                {
                                    if (el2.GetAttribute("value").Equals("Takibi bırak"))
                                    {
                                        el2.InvokeMember("Click");
                                    }
                                }
                            }

                            // Takipten çıkarıldığında
                            else if (web1_HtmlKod.IndexOf("<INPUT type=submit value=\"Takip et\" name=commit>") != -1 || web1_HtmlKod.IndexOf("<INPUT type=submit value=\"İstek gönder\" name=commit>") != -1)
                            {
                                // Bilgi mesajları
                                bilgiMesajGonder(gecerliProfil + " profili takip etme bırakıldı.", "Bilgi");
                                listBox_TakiptenCikarma_İslemPenceresi.Items.Insert(0, gecerliProfil + " profili takip etme bırakıldı.");

                                // Değişken atama
                                takiptenCikarma_Kalan = int.Parse(textBox_TakiptenCikarma_Toplam.Text) - (takiptenCikarma_Cikarilan + takiptenCikarma_Basarisiz);
                                takiptenCikarma_Toplam = takiptenCikarma_Kalan;
                                takiptenCikarma_Cikarilan = takiptenCikarma_Cikarilan + 1;

                                // profil seçme
                                listBox_TakiptenCikarma_TakipListem.SelectedIndex = listBox_TakiptenCikarma_TakipListem.SelectedIndex + 1;

                                // Tarama Başlat
                                tTakiptenCikarma = new Thread(delegate()
                                {
                                    takiptenCikarma(listBox_TakiptenCikarma_TakipListem.Text);
                                });
                                tTakiptenCikarma.Start();
                            }
                                // vb hatalar
                            else if (web1_HtmlKod.IndexOf("Üzgünüz, böyle bir sayfa yok") != -1 || web1_HtmlKod.IndexOf("Bu hesap şu an için askıya alınmış") != -1)
                            {
                                // Bilgi mesajları
                                bilgiMesajGonder(gecerliProfil + " profili atlandı.", "Bilgi");
                                listBox_TakiptenCikarma_İslemPenceresi.Items.Insert(0, gecerliProfil + " profili atlandı.");

                                // Değişken atama
                                takiptenCikarma_Kalan = int.Parse(textBox_TakiptenCikarma_Toplam.Text) - (takiptenCikarma_Cikarilan + takiptenCikarma_Basarisiz);
                                takiptenCikarma_Toplam = takiptenCikarma_Kalan;
                                takiptenCikarma_Cikarilan = takiptenCikarma_Cikarilan + 1;

                                // profil seçme
                                listBox_TakiptenCikarma_TakipListem.SelectedIndex = listBox_TakiptenCikarma_TakipListem.SelectedIndex + 1;

                                // Tarama Başlat
                                tTakiptenCikarma = new Thread(delegate()
                                {
                                    takiptenCikarma(listBox_TakiptenCikarma_TakipListem.Text);
                                });
                                tTakiptenCikarma.Start();
                            }

                        }

                    }

                    // Detaylar
                    textBox_TakiptenCikarma_Kalan.Text = takiptenCikarma_Kalan.ToString();
                    textBox_TakiptenCikarma_Cikarilan.Text = takiptenCikarma_Cikarilan.ToString();
                    textBox_TakiptenCikarma_Basarisiz.Text = takiptenCikarma_Basarisiz.ToString();
                }




            }
            catch
            {
                // Bilgi Mesajları
                bilgiMesajGonder("Siteye bağlanılamıyor!", "hata");
            }
        }
        #endregion

        #region webBrowser2 Ekstra işlemler [DocumentCompleted]
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // webbrowser kaynak kod
            web2_HtmlKod = webBrowser2.Document.Body.InnerHtml.ToString();
            richTextBox2.Text = web2_HtmlKod;

            // geçerli sayfa
            textBox2.Text = webBrowser2.Url.ToString();

            // Gündem sayfası ise
            if (web2_HtmlKod.IndexOf("Gündem") != -1)
            {
                // Gündem verileri
                try
                {
                    listBox_DunyaGundemi.Items.Clear();

                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(web2_HtmlKod);

                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//*[@id=\"main_content\"]//a/text()");
                    foreach (var veri2 in XPath)
                    {
                        if (veri2.InnerHtml != "Konum değiştir")
                        {
                            // listeye ekleme
                            listBox_DunyaGundemi.Items.Add(veri2.InnerHtml);
                        }
                    }

                    // Bilgi mesajları
                    bilgiMesajGonder("Türkiye Gündemi başarıyla çekildi.", "Başarılı");
                }
                catch
                {
                    // Bilgi mesajları
                    bilgiMesajGonder("Türkiye Gündemi çekilemiyor. Yenile butonuna tıklayarak yeniden deneyebilirsiniz.", "Hata");
                    //MessageBox.Show("Dünya gündemi çekilemiyor. Yenile butonuna tıklayarak yeniden deneyebilirsiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
        #endregion

        #region Takipçi Toplama

        #region Profil Listesi Yükle butonu
        private void button_TakipciToplama_ProfilYukle_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog f = new OpenFileDialog();
                f.Title = "Profil Listesi Yükle";
                f.Filter = "Txt dosyası |*.txt";
                if (f.ShowDialog() == DialogResult.OK)
                {
                    listBox_TakipciToplama_ProfilListesi.Items.Clear();

                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(f.OpenFile()))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            listBox_TakipciToplama_ProfilListesi.Items.Add(line);
                        }
                    }

                    // Bilgi mesajları
                    bilgiMesajGonder("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakipciToplama_ProfilListesi.Items.Count.ToString(), "başarılı");
                    MessageBox.Show("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakipciToplama_ProfilListesi.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                // Bilgi mesajları
                bilgiMesajGonder("Profil Listesi yüklenemedi. Tekrar deneyin..", "hata");
                MessageBox.Show("Profil Listesi yüklenemedi. Tekrar deneyin..", "Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Başlat Butonu
        private void button_TakipciToplama_Baslat_Click(object sender, EventArgs e)
        {
            // profil listesinde profil varmı
            if (listBox_TakipciToplama_ProfilListesi.Items.Count > 0)
            {
                 DialogResult soru = MessageBox.Show("Kaynak profilden takipçi toplama işlemi başlasın mı?\n\nNOT: Bu işlemin uzunluğu Yüklenen Profil Listesi göre değişecektir. Ayrıca internet hızı ve bilgisayar hızınada bağlıdır. Dilerseniz durdurabilir ve listesi kaydedebilirsiniz.", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                 if (soru == DialogResult.Yes)
                 {
                     // temizlik
                     listBox_TakipciToplama_TakipciListesi.Items.Clear();
                     textBox_TakipciToplama_CekilenProfil.Text = "null";
                     textBox_TakipciToplama_KalanProfil.Text = "null";
                     textBox_TakipciToplama_CekilenProfil.Text = "null";

                     // Nesne pasifleştir
                     button_TakipciToplama_ProfilYukle.Enabled = false;
                     listBox_TakipciToplama_ProfilListesi.Enabled = false;
                     button_TakipciToplama_Baslat.Enabled = false;
                     button_TakipciToplama_ListeyiKaydet.Enabled = false;

                     // Nesne aktifleştir
                     button_TakipciToplama_Durdur.Enabled = true;

                     // değişkene atama
                     hesapToplamaAktif = true;
                     hesapToplamaBaslamaZamani = DateTime.Now.ToString();

                     // Bilgi mesajları
                     bilgiMesajGonder("Kaynak profilden takipçi toplama işlemi başladı.","Bilgi");

                     // profil seçme
                     listBox_TakipciToplama_ProfilListesi.SelectedIndex = listBox_TakipciToplama_ProfilListesi.SelectedIndex + 1;

                     // Tarama Başlat
                     tTakipciToplama = new Thread(delegate()
                     {
                         takipciToplama(listBox_TakipciToplama_ProfilListesi.Text);
                     });
                     tTakipciToplama.Start();

                 }
            }
            else
            {
                // Bilgi mesajları
                bilgiMesajGonder("Takipçi toplama için Profil Listesi eklemelisiniz.", "bilgi");
                MessageBox.Show("Takipçi toplama için Profil Listesi eklemelisiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Durdur butonu
        private void button_TakipciToplama_Durdur_Click(object sender, EventArgs e)
        {
            DialogResult soru = MessageBox.Show("Kaynak profilden takipçi toplama işlemi durdurulsun mu?\n\nNOT: Listeyi kaydetmeyi unutmayın..", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Nesne pasifleştir
                button_TakipciToplama_Durdur.Enabled = false;

                // Nesne aktifleştir
                button_TakipciToplama_ProfilYukle.Enabled = true;
                listBox_TakipciToplama_ProfilListesi.Enabled = true;
                button_TakipciToplama_Baslat.Enabled = true;
                button_TakipciToplama_ListeyiKaydet.Enabled = true;

                // değişkene atama
                hesapToplamaAktif = false;
                hesapToplamaBaslamaZamani = null;

                // Bilgi mesajları
                bilgiMesajGonder("Kaynak profilden takipçi toplama işlemi durduruldu.", "Bilgi");
            
                // Tarama durdur
                tTakipciToplama.Abort();
            }
        }
        #endregion

        #region Listeyi kaydet
        private void button_TakipciToplama_ListeyiKaydet_Click(object sender, EventArgs e)
        {
            SaveFileDialog kaydetPencere = new SaveFileDialog();
            kaydetPencere.Title = "Kaydedilecek yeri seçin";
            kaydetPencere.Filter = "Txt dosyası |*.txt";

            if (kaydetPencere.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter kaydet = new StreamWriter(kaydetPencere.FileName);
                    foreach (var item in listBox_TakipciToplama_TakipciListesi.Items)
                    {
                        kaydet.WriteLine(item);
                    }
                    kaydet.Close();

                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya başarıyla kaydedildi.", "başarılı");
                    MessageBox.Show("Dosya başarıyla kaydedildi.\nDosya yolu: " + kaydetPencere.FileName, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch
                {
                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya kaydedilemiyor. Tekrar deneyin..", "hata");
                    MessageBox.Show("Dosya kaydedilemiyor. Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }
        #endregion

        void takipciToplama(string profil, string cursor = "-1")
        {
            string url = "https://mobile.twitter.com/" + profil + "/followers?cursor=" + cursor;
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string html = client.DownloadString(url);


            // istatistik bilgiler
            //textBox_TakipciToplama_ToplamProfil.Text = listBox_TakipciToplama_ProfilListesi.Items.Count.ToString();
            //textBox_TakipciToplama_KalanProfil.Text = (listBox_TakipciToplama_ProfilListesi.Items.Count - listBox_TakipciToplama_TakipciListesi.se).ToString();

            // hesapları çekip listeye ekleme
            try
            {
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);

                HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//span[@class='username']");
                foreach (var veri2 in XPath)
                {
                    string hesap = veri2.InnerText.Remove(0, 1);
                    if (hesap.IndexOf(bagliHesap) != -1)
                    { }
                    else
                    {
                        if (listBox_TakipciToplama_TakipciListesi.Items.Contains(hesap) == false)
                        {
                            // listbox'a ekle
                            listBox_TakipciToplama_TakipciListesi.Items.Add(hesap.Trim());
                        }
                        // toplam profil sayısı
                        textBox_TakipciToplama_CekilenProfil.Text = listBox_TakipciToplama_TakipciListesi.Items.Count.ToString();
                    }
                }
            } catch { }

            // cursor yakalama
            try
            {
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);

                HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='w-button-more']");
                foreach (var veri2 in XPath)
                {
                    // veri ayıklayarak cursor çekme
                    veriAyiklama(veri2.InnerHtml, "cursor=", 7, "\">");

                    // ayıklanan veri ile fonksiyon döndürme
                    // Tarama Başlat
                    tTakipciToplama = new Thread(delegate()
                    {
                        takipciToplama(profil, veri);
                    });
                    tTakipciToplama.Start();
                }
            }
            catch
            {
                // Kaynak profil listesi tamamsa durdur
                if (listBox_TakipciToplama_ProfilListesi.Items.Count == listBox_TakipciToplama_ProfilListesi.SelectedIndex + 1)
                {
                    // Nesne pasifleştir
                    button_TakipciToplama_Durdur.Enabled = false;

                    // Nesne aktifleştir
                    button_TakipciToplama_ProfilYukle.Enabled = true;
                    listBox_TakipciToplama_ProfilListesi.Enabled = true;
                    button_TakipciToplama_Baslat.Enabled = true;
                    button_TakipciToplama_ListeyiKaydet.Enabled = true;

                    // değişkene atama
                    hesapToplamaAktif = false;
                    hesapToplamaBaslamaZamani = null;

                    // Bilgi mesajları
                    bilgiMesajGonder("Kaynak profilden takipçi toplama işlemi tamamlandı. Toplam Profil: " + listBox_TakipciToplama_TakipciListesi.Items.Count.ToString(), "başarılı");
                    MessageBox.Show("Kaynak profilden takipçi toplama işlemi tamamlandı. Toplam Profil: " + listBox_TakipciToplama_TakipciListesi.Items.Count.ToString(),"Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);
                   
                    // Tarama durdur
                    tTakipciToplama.Abort();
                }
                else
                {
                    // Bir diğer profile geçme
                    // profil seçme
                    listBox_TakipciToplama_ProfilListesi.SelectedIndex = listBox_TakipciToplama_ProfilListesi.SelectedIndex + 1;

                    // Tarama Başlat
                    takipciToplama(listBox_TakipciToplama_ProfilListesi.Text);
                }
            }
            
        }

        #endregion

        #region Timer İşlemleri
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Güncel zaman
            DateTime guncelZaman = DateTime.Now;

            // Twitter engellediğinde
            if (twitterEngelAktif == true)
            {
                if (twitterEngelSure == guncelZaman.ToString())
                {
                    // toplu takipten çıkarma
                    if (takiptenCikarmaAktif == true)
                    {
                        // Tarama Başlat
                        tTakiptenCikarma = new Thread(delegate()
                        {
                            takiptenCikarma(listBox_TakiptenCikarma_TakipListem.Text);
                        });
                        tTakiptenCikarma.Start();
                    }

                    // toplu takip etme
                    if (takipEtmeAktif == true)
                    {
                        // Tarama Başlat
                        tTakipEtme = new Thread(delegate()
                        {
                            takipEtme(listBox_TakipEtme_ProfilListesi.Text);
                        });
                        tTakipEtme.Start();

                        // Değişken atama
                        twitterEngelAktif = false;
                    }
                }
            }

            // hesap toplama
            if (hesapToplamaAktif == true)
            {
                string tarih1 = DateTime.Now.ToString();
                string tarih2 = hesapToplamaBaslamaZamani;

                TimeSpan ts = (Convert.ToDateTime(tarih1) - Convert.ToDateTime(tarih2));
                label_TakipciToplama_GecenZaman.Text = "Geçen Zaman: [" + String.Format(@"{0:hh\:mm\:ss}", ts) + "]";
            }

            // toplu takip etme
            if (takipEtmeAktif == true)
            {
                string tarih3 = DateTime.Now.ToString();
                string tarih4 = takipEtmeBaslamaZamani;

                TimeSpan ts2 = (Convert.ToDateTime(tarih3) - Convert.ToDateTime(tarih4));
                label_TakipEtme_GecenZaman.Text = "Geçen Zaman: [" + String.Format(@"{0:hh\:mm\:ss}", ts2) + "]";
            }

            // toplu takipten çıkarma
            if (takiptenCikarmaAktif == true)
            {
                string tarih5 = DateTime.Now.ToString();
                string tarih6 = takiptenCikarmaBaslamaZamani;

                TimeSpan ts3 = (Convert.ToDateTime(tarih5) - Convert.ToDateTime(tarih6));
                label_TakiptenCikarma_GecenZaman.Text = "Geçen Zaman: [" + String.Format(@"{0:hh\:mm\:ss}", ts3) + "]";
            }
        }
        #endregion

        #region Toplu Takip Etme İşlemleri

        #region Liste Yükle butonu
        private void button_TakipEtme_ListeYükle_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog f = new OpenFileDialog();
                f.Title = "Profil Listesi Yükle";
                f.Filter = "Txt dosyası |*.txt";

                if (f.ShowDialog() == DialogResult.OK)
                {
                    listBox_TakipEtme_ProfilListesi.Items.Clear();

                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(f.OpenFile()))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            listBox_TakipEtme_ProfilListesi.Items.Add(line);
                        }
                    }

                    // Bilgi mesajları
                    bilgiMesajGonder("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakipEtme_ProfilListesi.Items.Count.ToString(), "başarılı");
                    MessageBox.Show("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakipEtme_ProfilListesi.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                // Bilgi mesajları
                bilgiMesajGonder("Profil Listesi yüklenemedi. Tekrar deneyin..", "hata");
                MessageBox.Show("Profil Listesi yüklenemedi. Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Başlat Butonu
        private void button_TakipEtme_Baslat_Click(object sender, EventArgs e)
        {
            // profil listesinde profil varmı
            if (listBox_TakipEtme_ProfilListesi.Items.Count > 0)
            {
                DialogResult soru = MessageBox.Show("Toplu Takip Etme işlemi başlasın mı?\n\nNOT: Bu işlemin uzunluğu Yüklenen Profil Listesi göre değişecektir. Ayrıca internet hızı ve bilgisayar hızınada bağlıdır.", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // temizlik
                    listBox_TakipEtme_DurumPenceresi.Items.Clear();
                    textBox_TakipEtme_ToplamProfil.Text = "null";
                    textBox_TakipEtme_KalanProfil.Text = "null";
                    textBox_TakipEtme_TakipEdilen.Text = "null";
                    textBox_TakipEtme_Basarisiz.Text = "null";
                    label_TakipEtme_Durum.Text = "Bekleniyor..";

                    // Nesne pasifleştir
                    button_TakipEtme_ListeYükle.Enabled = false;
                    listBox_TakipEtme_ProfilListesi.Enabled = false;
                    button_TakipEtme_Baslat.Enabled = false;

                    // Nesne aktifleştir
                    button_TakipEtme_Durdur.Enabled = true;

                    // değişkene atama
                    takipEtmeAktif = true;
                    takipEtmeBaslamaZamani = DateTime.Now.ToString();
                    takipEtme_Kalan = 0;
                    takipEtme_TakipEdilen = 0;
                    takipEtme_Toplam = 0;
                    takipEtme_Basarisiz = 0;
                    takipEtme_Toplam = listBox_TakiptenCikarma_TakipListem.Items.Count;

                    // Bilgi mesajları
                    bilgiMesajGonder("Toplu Takip Etme işlemi başladı.", "Bilgi");

                    // profil seçme
                    listBox_TakipEtme_ProfilListesi.SelectedIndex = -1;
                    listBox_TakipEtme_ProfilListesi.SelectedIndex = listBox_TakipEtme_ProfilListesi.SelectedIndex + 1;

                    // Tarama Başlat
                    tTakipEtme = new Thread(delegate()
                    {
                        takipEtme(listBox_TakipEtme_ProfilListesi.Text);
                    });
                    tTakipEtme.Start();
                }
            }
            else
            {
                // Bilgi mesajları
                bilgiMesajGonder("Toplu Takip Etme için Profil Listesi yüklemelisiniz.", "bilgi");
                MessageBox.Show("Toplu Takip Etme için Profil Listesi yüklemelisiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Durdur butonu
        private void button_TakipEtme_Durdur_Click(object sender, EventArgs e)
        {
            DialogResult soru = MessageBox.Show("Toplu Takip Etme işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Nesne aktifleştir
                button_TakipEtme_ListeYükle.Enabled = true;
                listBox_TakipEtme_ProfilListesi.Enabled = true;
                button_TakipEtme_Baslat.Enabled = true;

                // Nesne pasifleştir
                button_TakipEtme_Durdur.Enabled = false;

                // değişkene atama
                takipEtmeAktif = false;
                takipEtmeBaslamaZamani = null;

                // Bilgi mesajları
                bilgiMesajGonder("Toplu Takip Etme işlemi durduruldu.", "Bilgi");
            
                // Tarama Durdur
                tTakipEtme.Abort();
            }
        }
        #endregion

        void takipEtme(string profil)
        {
            // Profile bağlanma
            webBrowser1.Navigate("https://mobile.twitter.com/" + profil);
        }

        #endregion

        #region Takip Edilen Profilleri Toplama İşlemi

        #region Takipçileri çek Checkbox Butonu
        private void checkBox_TakiptenCikarma_TakipcileriCek_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_TakiptenCikarma_TakipEttiklerimiCek.CheckState == CheckState.Checked)
            {
                DialogResult soru = MessageBox.Show("Takip Ettiğim Profiller çekilsin mi?\n\nNOT: Bu işlemin uzunluğu Yüklenen Profil Listesi göre değişecektir. Ayrıca internet hızı ve bilgisayar hızınada bağlıdır.", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // Temizlik
                    listBox_TakiptenCikarma_TakipListem.Items.Clear();
                    textBox_TakiptenCikarma_Toplam.Text = "null";

                    // Nesne aktifleştirme
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.ForeColor = Color.DarkRed;
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.Text = "Takip Ettiklerimi Çekmeyi Durdur";

                    // Nesne pasifleştirme
                    button_TakiptenCikarma_ListeYukle.Enabled = false;
                    button_TakiptenCikarma_ListeKaydet.Enabled = false;

                    // Bilgi mesajları
                    bilgiMesajGonder("Takip ettiğim profiller toplanmaya başlandı.", "Bilgi");

                    // Değişkene atama
                    takipEttigimToplamaAktif = true;

                    // Tarama Başlat
                    tTakipEttigimToplama = new Thread(delegate()
                    {
                        takipcilerimiCek();
                    });
                    tTakipEttigimToplama.Start();
                }
            }
            else
            {
                DialogResult soru = MessageBox.Show("Takip Ettiğim Profilleri çekme işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // Nesne aktifleştirme
                    button_TakiptenCikarma_ListeYukle.Enabled = true;
                    button_TakiptenCikarma_ListeKaydet.Enabled = true;

                    // Nesne pasifleştirme
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.ForeColor = Color.DarkGreen;
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.Text = "Takip Ettiklerimi Çekmeyi Başlat";

                    // Değişkene atama
                    takipEttigimToplamaAktif = false;

                    // Bilgi mesajları
                    bilgiMesajGonder("Takip ettiğim profilleri toplama işlemi durduruldu.", "Bilgi");

                    // Tarama Başlat
                    tTakipEttigimToplama.Abort();
                }
            }
        }
        #endregion

        #region Liste Yükle butonu
        private void button_TakiptenCikarma_ListeYukle_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog f = new OpenFileDialog();
                f.Title = "Profil Listesi Yükle";
                f.Filter = "Txt dosyası |*.txt";
                if (f.ShowDialog() == DialogResult.OK)
                {
                    listBox_TakiptenCikarma_TakipListem.Items.Clear();

                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(f.OpenFile()))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            if (line.IndexOf(bagliHesap) != -1) 
                            {

                            }
                            else
                            {
                                listBox_TakiptenCikarma_TakipListem.Items.Add(line);
                            }
                        }
                    }

                    textBox_TakiptenCikarma_Toplam.Text = listBox_TakiptenCikarma_TakipListem.Items.Count.ToString();

                    // Bilgi mesajları
                    bilgiMesajGonder("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakiptenCikarma_TakipListem.Items.Count.ToString(), "başarılı");
                    MessageBox.Show("Profil Listesi başarıyla yüklendi. Toplam: " + listBox_TakiptenCikarma_TakipListem.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                // Bilgi mesajları
                bilgiMesajGonder("Profil Listesi yüklenemedi. Tekrar deneyin..", "hata");
                MessageBox.Show("Profil Listesi yüklenemedi. Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Liste kaydet butonu
        private void button_TakiptenCikarma_ListeKaydet_Click(object sender, EventArgs e)
        {
            SaveFileDialog kaydetPencere = new SaveFileDialog();
            kaydetPencere.Title = "Kaydedilecek yeri seçin";
            kaydetPencere.Filter = "Txt dosyası |*.txt";

            if (kaydetPencere.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter kaydet = new StreamWriter(kaydetPencere.FileName);
                    foreach (var item in listBox_TakiptenCikarma_TakipListem.Items)
                    {
                        kaydet.WriteLine(item);
                    }
                    kaydet.Close();

                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya başarıyla kaydedildi.", "başarılı");
                    MessageBox.Show("Dosya başarıyla kaydedildi.\nDosya yolu: " + kaydetPencere.FileName, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch
                {
                    // Bilgi mesajları
                    bilgiMesajGonder("Dosya kaydedilemiyor. Tekrar deneyin..", "hata");
                    MessageBox.Show("Dosya kaydedilemiyor. Tekrar deneyin..", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }
        #endregion

        void takipcilerimiCek(string cursor = "-1")
        {
            try
            {
                string url = "https://mobile.twitter.com/" + bagliHesap + "/following?cursor=" + cursor;
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                string html = client.DownloadString(url);

                // hesapları çekip listeye ekleme
                try
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(html);

                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//span[@class='username']");
                    foreach (var veri2 in XPath)
                    {
                        string hesap = veri2.InnerText.Remove(0, 1);
                        if (hesap.ToLower().IndexOf(bagliHesap) != -1)
                        { }
                        else
                        {
                            if (listBox_TakiptenCikarma_TakipListem.Items.Contains(hesap) == false)
                            {
                                // listbox'a ekle
                                listBox_TakiptenCikarma_TakipListem.Items.Add(hesap.Trim());
                            }
                            // toplam profil sayısı
                            textBox_TakiptenCikarma_Toplam.Text = listBox_TakiptenCikarma_TakipListem.Items.Count.ToString();
                        }
                    }
                }
                catch { }

                // cursor yakalama
                try
                {
                    HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(html);

                    HtmlNodeCollection XPath = dokuman.DocumentNode.SelectNodes("//div[@class='w-button-more']");
                    foreach (var veri2 in XPath)
                    {
                        // veri ayıklayarak cursor çekme
                        veriAyiklama(veri2.InnerHtml, "cursor=", 7, "\">");

                        // ayıklanan veri ile fonksiyon döndürme
                        // Tarama Başlat
                        tTakipEttigimToplama = new Thread(delegate()
                        {
                            takipcilerimiCek(veri);
                        });
                        tTakipEttigimToplama.Start();
                    }
                }
                catch
                {

                    // Nesne aktifleştirme
                    button_TakiptenCikarma_ListeYukle.Enabled = true;
                    button_TakiptenCikarma_ListeKaydet.Enabled = true;

                    // Nesne pasifleştirme
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.ForeColor = Color.DarkGreen;
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.Text = "Takip Ettiklerimi Çekmeyi Başlat";

                    // Değişkene atama
                    takipEttigimToplamaAktif = false;

                    // Bilgi mesajları
                    bilgiMesajGonder("Takip Ettiğim profilleri çekme işlemi tamamlandı. Toplam Profil: " + listBox_TakiptenCikarma_TakipListem.Items.Count.ToString(), "başarılı");
                    MessageBox.Show("Takip Ettiğim profilleri çekme işlemi tamamlandı. Toplam Profil: " + listBox_TakiptenCikarma_TakipListem.Items.Count.ToString(), "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tarama durdur
                    tTakipEttigimToplama.Abort();
                }
            }
            catch { }
        }

        #endregion

        #region  Toplu Takipten Çıkarma İşlemleri

        #region Başlat butonu
        private void button_TakiptenCikarma_Baslat_Click(object sender, EventArgs e)
        {
            // profil listesinde profil varmı
            if (listBox_TakiptenCikarma_TakipListem.Items.Count > 0)
            {
                DialogResult soru = MessageBox.Show("Toplu Takipten Çıkarma işlemi başlasın mı?\n\nNOT: Bu işlemin uzunluğu Yüklenen Profil Listesi göre değişecektir. Ayrıca internet hızı ve bilgisayar hızınada bağlıdır.", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (soru == DialogResult.Yes)
                {
                    // temizlik
                    listBox_TakiptenCikarma_İslemPenceresi.Items.Clear();
                    textBox_TakiptenCikarma_Kalan.Text = "null";
                    textBox_TakiptenCikarma_Cikarilan.Text = "null";
                    textBox_TakiptenCikarma_Basarisiz.Text = "null";
                    label_TakiptenCikarma_Durum.Text = "Bekleniyor..";

                    // Nesne pasifleştir
                    button_TakiptenCikarma_Baslat.Enabled = false;
                    checkBox_TakiptenCikarma_TakipEttiklerimiCek.Enabled = false;
                    button_TakiptenCikarma_ListeYukle.Enabled = false;
                    button_TakiptenCikarma_ListeKaydet.Enabled = false;
                    listBox_TakiptenCikarma_TakipListem.Enabled = false;

                    // Nesne aktifleştir
                    button_TakiptenCikarma_Durdur.Enabled = true;

                    // değişkene atama
                    takiptenCikarmaAktif = true;
                    takiptenCikarmaBaslamaZamani = DateTime.Now.ToString();
                    takiptenCikarma_Toplam = 0;
                    takiptenCikarma_Kalan = 0;
                    takiptenCikarma_Cikarilan = 0;
                    takiptenCikarma_Basarisiz = 0;
                    takiptenCikarma_Toplam = listBox_TakiptenCikarma_TakipListem.Items.Count;

                    // Bilgi mesajları
                    bilgiMesajGonder("Toplu Takipten Çıkarma işlemi başladı.", "Bilgi");

                    // profil seçme
                    listBox_TakiptenCikarma_TakipListem.SelectedIndex = -1;
                    listBox_TakiptenCikarma_TakipListem.SelectedIndex = listBox_TakiptenCikarma_TakipListem.SelectedIndex + 1;

                    // Tarama Başlat
                    tTakiptenCikarma = new Thread(delegate()
                    {
                        takiptenCikarma(listBox_TakiptenCikarma_TakipListem.Text);
                    });
                    tTakiptenCikarma.Start();
                }
            }
            else
            {
                // Bilgi mesajları
                bilgiMesajGonder("Toplu Takipten Çıkarma için Profil Listesi yüklemelisiniz.", "bilgi");
                MessageBox.Show("Toplu Takipten Çıkarma için Profil Listesi yüklemelisiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Durdur butonu
        private void button_TakiptenCikarma_Durdur_Click(object sender, EventArgs e)
        {
            DialogResult soru = MessageBox.Show("Toplu Takipten Çıkarma işlemi durdurulsun mu?", "Soru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (soru == DialogResult.Yes)
            {
                // Nesne aktifleştir
                checkBox_TakiptenCikarma_TakipEttiklerimiCek.Enabled = true;
                button_TakiptenCikarma_ListeYukle.Enabled = true;
                button_TakiptenCikarma_ListeKaydet.Enabled = true;
                button_TakiptenCikarma_Baslat.Enabled = true;

                // Nesne pasifleştir
                button_TakiptenCikarma_Durdur.Enabled = false;

                // değişkene atama
                takiptenCikarmaAktif = false;
                takiptenCikarmaBaslamaZamani = null;

                // Bilgi mesajları
                bilgiMesajGonder("Toplu Takipten Çıkarma işlemi durduruldu.", "Bilgi");

                // Tarama Durdur
                tTakiptenCikarma.Abort();
            }
        }
        #endregion

        void takiptenCikarma(string profil)
        {
            Thread.Sleep(3000);

            // Profile bağlanma
            webBrowser1.Navigate("https://mobile.twitter.com/" + profil);
        }

        #endregion










    }
}
