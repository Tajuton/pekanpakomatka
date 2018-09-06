using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;


/// @author Tuukka Jurvakainen
/// @version 1.0
/// Pekan pakomatka niminen tasohyppelypeli
public class PekanPakomatka : PhysicsGame
{

    //Ladataan tarvittavat kuvat ja animaatiot:
    private Image ruoho = LoadImage("ruoho");
    private Image lahtokuva = LoadImage("ruotsilippu");
    private Image maalikuva = LoadImage("suomilippu");
    private Image kolikkokuva = LoadImage("kolikko");
    private Image maa = LoadImage("maasto");
    private Animation PekanKavely;
    private Animation VihollisenKavely;

    //luodaan listat liikkuvista laatoista ja vihollisista
    private List<PhysicsObject> laatat = new List<PhysicsObject>();
    private List<PlatformCharacter> viholliset = new List<PlatformCharacter>();

    //laattojen liikutusmuistinumero:
    private int liikutusmuisti = 0;
    private int vihujenliikutus = 0;

    //Luodaan pelaaja ja vihollisen mallityyppi
    private PlatformCharacter pelaaja;
    private PlatformCharacter vihollinen;

    //TopTen lista:
    private EasyHighScore parhaatLista = new EasyHighScore();

    //Muuttujia:
    private IntMeter pisteLaskuri;
    private IntMeter elamaLaskuri;
    private int kentta = 1;
    private string kentannimi;
    private int elamat = 3;
    private int pisteet = 0;
    private int kenttapisteet = 0;
    private int valikko = 1;


    /// <summary>
    /// Aliohjelma, joka ajetaan pelin aluksi. Ladataan animaatiot ja valikko.
    /// </summary>
    public override void Begin()
    {
        //Ladataan animaatiot:
        PekanKavely = LoadAnimation("pekka");
        VihollisenKavely = LoadAnimation("vihollinen");

        //kutsutaan 
        Valikko();

    }


    /// <summary>
    /// Aliohjelma, joka tekee alkuvalikon
    /// </summary>
    private void Valikko()
    {
        MultiSelectWindow alkuValikko = new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Parhaat pisteet", "Lopeta");
        Add(alkuValikko);
        alkuValikko.AddItemHandler(0, KentanVaihto);
        alkuValikko.AddItemHandler(1, TopLista);
        alkuValikko.AddItemHandler(2, Exit);
    }


    /// <summary>
    /// Aliohjelma, joka lisää pistepisteLaskurin peliin
    /// </summary>
    /// <param name="laskuri">Laskuri, joka luodaan</param>
    /// <param name="otsikko">Laskurin otsikko, näkyy tekstinä pelissä</param>
    /// <param name="paikkaX">Laskurin paikka X-suunnassa(ruudun vasen yläkulma 0,0)</param>
    /// <param name="paikkaY">Laskurin paikka Y-suunnassa(ruudun vasen yläkulma 0,0)</param>
    private IntMeter LisaaLaskuri(IntMeter laskuri, int paikkaX = 0, int paikkaY = 0, string otsikko = "")
    {

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + paikkaX;
        pisteNaytto.Y = Screen.Top - paikkaY;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = otsikko;

        pisteNaytto.BindTo(laskuri);
        Add(pisteNaytto);
        return laskuri;
    }


    /// <summary>
    /// Aliohjelma, jossa asetetaan ohjaimet pelaajalle
    /// </summary>
    /// <param name="pelaaja">pelihahmo</param>
    private void AsetaOhjaimet(PlatformCharacter pelaaja)
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaVasemmalle, "Pelaaja liikkuu vasemmalle");
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaOikealle, "Pelaaja liikkuu oikealle");
        Keyboard.Listen(Key.Space, ButtonState.Down, Hyppaa, "Pelaaja hyppaa");
    }


    /// <summary>
    /// Aliohjelma, jossa päivitetään/tarkistetaan pelitapahtumia jatkuvasti
    /// </summary>
    /// <param name="time">Peliaika</param>
    protected override void Update(Time Time)
    {

        base.Update(Time);

        if (valikko == -1)
        {
            LiikutaLaattoja();
            LiikutaVihollisia();
            pisteLaskuri.Value = pisteet;
            elamaLaskuri.Value = elamat;

            if (pelaaja.Y < -Level.Height / 2 - 20)
            {
                elamat--;
                if (elamat > 0)
                {
                    pisteet = pisteet - kenttapisteet;
                    KentanVaihto();
                }
            }

            if (kentta > 3)
            {
                kentta = 1;
                PelaajaKuoli();
            }
            if (elamat <= 0)
            {
                PelaajaKuoli();
            }

        }


    }


    /// <summary>
    /// Aliohjelma joka liikuttaa liikkuvia laattoja edes takaisin x-akselin suuntaisesti
    /// </summary>
    private void LiikutaLaattoja()
    {
        liikutusmuisti++;
        for (int i = 0; i < laatat.Count; i++)
        {
            if (liikutusmuisti >= 200)
            {
                liikutusmuisti = 0;
            }
            if (liikutusmuisti < 100)
            {
                laatat[i].Velocity = new Vector(-50, 0);
            }
            if (liikutusmuisti >= 100)
            {
                laatat[i].Velocity = new Vector(50, 0);
            }


        }
    }


    /// <summary>
    /// Aliohjelma joka liikuttaa vihollisa edes takaisin
    /// </summary>
    private void LiikutaVihollisia()
    {
        vihujenliikutus++;
        for (int i = 0; i < viholliset.Count; i++)
        {
            if (vihujenliikutus >= 132)
            {
                vihujenliikutus = 0;
            }
            if (vihujenliikutus < 66)
            {
                viholliset[i].Walk(-48);
            }
            if (vihujenliikutus >= 66)
            {
                viholliset[i].Walk(48);
            }


        }
    }


    /// <summary>
    /// Liikuttaa pelaajaa vasemmalle
    /// </summary>
    private void LiikutaVasemmalle()
    {
        pelaaja.Walk(-100);
    }


    /// <summary>
    /// Liikuttaa pelaajaa oikealle
    /// </summary>
    private void LiikutaOikealle()
    {
        pelaaja.Walk(100);
    }


    /// <summary>
    /// Tekee hypyn
    /// </summary>
    private void Hyppaa()
    {
        pelaaja.Jump(300);
    }


    /// <summary>
    /// Aliohjelma, joka luo kentän
    /// </summary>
    private void LuoKentta()
    {
        laatat.Clear();
        Level.Background.Color = Color.LightBlue;

        if (kentta == 1) kentannimi = "kentta1";
        if (kentta == 2) kentannimi = "kentta3";
        if (kentta == 3) kentannimi = "kentta2";
        if (kentta > 3) kentannimi = "kentta1";

        ColorTileMap ruudut = ColorTileMap.FromLevelAsset(kentannimi);
        ruudut.SetTileMethod(Color.Blue, LuoPelaaja);
        ruudut.SetTileMethod(new Color(0, 255, 0), LuoRuoho);
        ruudut.SetTileMethod(Color.Black, LuoMaa);
        ruudut.SetTileMethod(new Color(0, 255, 255), LuoMaali);
        ruudut.SetTileMethod(Color.Yellow, LuoLiikkuvaMaa, laatat);
        ruudut.SetTileMethod(Color.White, LuoKolikko);
        ruudut.SetTileMethod(Color.Red, LuoVihollinen);

        ruudut.Optimize(Color.Black);
        ruudut.Execute(20, 20);

    }


    /// <summary>
    /// Aliohjelma, jossa määritetään pelihahmon ominaisuuksia
    /// </summary>
    /// <param name="paikka">Pelihahmon paikka</param>
    /// <param name="leveys">Pelihahmon leveys</param>
    /// <param name="korkeus">Pelihahmon korkeus</param>
    private void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lahto = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        lahto.Position = paikka;
        lahto.Image = lahtokuva;
        lahto.CollisionIgnoreGroup = 2;
        Add(lahto, -1);

        pelaaja = new PlatformCharacter(20, 20, Shape.Circle);
        pelaaja.Position = paikka;
        pelaaja.AnimWalk = new Animation(PekanKavely);
        pelaaja.AnimIdle = new Animation(PekanKavely);
        pelaaja.Mass = 10;
        pelaaja.CollisionIgnoreGroup = 2;
        AddCollisionHandler(pelaaja, "kolikko", PelaajaOsuuKolikkoon);
        AddCollisionHandler(pelaaja, "maali", PelaajaOsuuMaaliin);
        AddCollisionHandler(pelaaja, "vihollinen", PelaajaOsuuViholliseen);
        Add(pelaaja);

    }


    /// <summary>
    /// Aliohjelma, joka luo ruohon
    /// </summary>
    /// <param name="paikka">Mihin luodaan ruoho</param>
    /// <param name="leveys">Ruohon leveys</param>
    /// <param name="korkeus">Ruohon korkeus</param>
    private void LuoRuoho(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = ruoho;
        taso.CollisionIgnoreGroup = 1;
        Add(taso);

    }


    /// <summary>
    /// Aliohjelma, joka luo maalin
    /// </summary>
    /// <param name="paikka">Mihin luodaan maali</param>
    /// <param name="leveys">Maalin leveys</param>
    /// <param name="korkeus">Maalin korkeus</param>
    private void LuoMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        maali.Position = paikka;
        maali.Image = maalikuva;
        maali.Tag = "maali";
        Add(maali, -1);

    }


    /// <summary>
    /// Aliohjelma, joka luo kolikon
    /// </summary>
    /// <param name="paikka">Mihin luodaan kolikko</param>
    /// <param name="leveys">Kolikon leveys</param>
    /// <param name="korkeus">Kolikon korkeus</param>
    private void LuoKolikko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kolikko = new PhysicsObject(leveys, korkeus, Shape.Circle);
        kolikko.Position = paikka;
        kolikko.Image = kolikkokuva;
        kolikko.CollisionIgnoreGroup = 3;
        kolikko.Tag = "kolikko";
        Add(kolikko, -1);
    }


    /// <summary>
    /// Aliohjelma, joka luo vihollisen
    /// </summary>
    /// <param name="paikka">Mihin luodaan vihollinen</param>
    /// <param name="leveys">Vihollisen leveys</param>
    /// <param name="korkeus">Vihollisen korkeus</param>
    private void LuoVihollinen(Vector paikka, double leveys, double korkeus)
    {
        vihollinen = new PlatformCharacter(20, 20, Shape.Circle);
        vihollinen.Position = paikka;
        vihollinen.AnimWalk = new Animation(VihollisenKavely);
        vihollinen.AnimIdle = new Animation(VihollisenKavely);
        vihollinen.Mass = 10;
        vihollinen.Tag = "vihollinen";
        Add(vihollinen);
        viholliset.Add(vihollinen);
    }


    /// <summary>
    /// Aliohjelma, joka luo maan
    /// </summary>
    /// <param name="paikka">Mihin luodaan maa</param>
    /// <param name="leveys">maan leveys</param>
    /// <param name="korkeus">maan korkeus</param>
    private void LuoMaa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = maa;
        taso.CollisionIgnoreGroup = 1;
        Add(taso);

    }


    /// <summary>
    /// Aliohjelma, joka luo liikkuvan maan
    /// </summary>
    /// <param name="paikka">Mihin luodaan liikkuva maapala</param>
    /// <param name="leveys">Liikuvan maan leveys</param>
    /// <param name="korkeus">Liikkuvan maan korkeus</param>
    private void LuoLiikkuvaMaa(Vector paikka, double leveys, double korkeus, List<PhysicsObject> laatat)
    {
        PhysicsObject liikkuvamaa = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Rectangle);
        liikkuvamaa.Position = paikka;
        liikkuvamaa.Image = ruoho;
        liikkuvamaa.CollisionIgnoreGroup = 1;
        Add(liikkuvamaa);
        laatat.Add(liikkuvamaa);

    }


    /// <summary>
    /// Aliohjelma, joka poistaa kolikon ja lisää pisteitä, jos pelaaja osuu kolikkoon
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="kohde">Kolikko johon törmätään</param>
    private void PelaajaOsuuKolikkoon(PhysicsObject pelaaja, PhysicsObject kolikko)
    {
        kolikko.Destroy();
        pisteet = pisteet + 50;
        kenttapisteet = kenttapisteet + 50;
    }


    /// <summary>
    /// Aliohjelma, joka vaihtaa kenttää
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="kohde">Kohde johon törmätään</param>
    private void PelaajaOsuuMaaliin(PhysicsObject pelaaja, PhysicsObject maali)
    {
        kentta++;
        elamat++;
        KentanVaihto();
    }


    /// <summary>
    /// Aliohjelma, joka toteutetaan pelaajan osuessa viholliseen.
    /// Vähennetään elämiä ja aloitetaan kenttä alusta.
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="kohde">Kohde johon törmätään</param>
    private void PelaajaOsuuViholliseen(PhysicsObject pelaaja, PhysicsObject maali)
    {
        elamat--;
        if (elamat > 0)
        {
            pisteet = pisteet - kenttapisteet;
            KentanVaihto();
        }
    }


    /// <summary>
    /// Aliohjelma, joka avaa parheaiden pisteiden listan kun pelaaja kuolee
    /// </summary>
    private void PelaajaKuoli()
    {
        parhaatLista.EnterAndShow(pisteet);
        parhaatLista.HighScoreWindow.Closed += AloitaPeli;
        valikko = 1;
        elamat = 3;

    }


    /// <summary>
    /// Aloittaa pelin alusta eli luo valikon
    /// </summary>
    /// <param name="sender"></param>
    private void AloitaPeli(Window sender)
    {
        Valikko();
    }


    /// <summary>
    /// Luo Parhaiden pisteiden listan
    /// </summary>
    private void TopLista()
    {
        parhaatLista.Show();
        parhaatLista.HighScoreWindow.Closed += AloitaPeli;
    }


    /// <summary>
    /// Aliohjelma, joka toteuttaa kentän vaihdon
    /// </summary>
    void KentanVaihto()
    {
        //Ei olla enää valikossa:
        valikko = -1;

        //Nollataan kaikki:
        ClearAll();

        //Luodaan kenttä:
        LuoKentta();

        //Asetetaan ohjaimet:
        AsetaOhjaimet(pelaaja);

        //Nollataan muistista liikutusmuistit ja edellisestä kentästä saadut pisteet:
        liikutusmuisti = 0;
        vihujenliikutus = 0;
        kenttapisteet = 0;

        //Lisää elämä- ja pistelaskuri:
        pisteLaskuri = LisaaLaskuri(new IntMeter(0), 100, 100, "Pisteet");
        elamaLaskuri = LisaaLaskuri(new IntMeter(0), 100, 130, "Elämät");

        //Painovoima:
        Gravity = new Vector(0, -400);

        //Kameran zoomaus ja asetetaan se seuraamaan pelaajaa:
        Camera.Zoom(2);
        Camera.FollowX(pelaaja);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Valikko, "Avaa alkuvalikon");
    }

}
