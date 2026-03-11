/// <summary>
/// Constantes globales del proyecto.
/// Centraliza todos los tags, nombres de audio y claves de PlayerPrefs
/// para eliminar magic strings y facilitar el mantenimiento.
/// </summary>
public static class GameConstants
{
    // ?? Tags ??????????????????????????????????????????????????????????????????
    public static class Tags
    {
        public const string Harry    = "Harry";
        public const string Dementor = "Dementor";
        public const string Hechizo  = "Hechizo";
        public const string Escoba   = "Escoba";
        public const string Buckbeak = "Buckbeak";
        public const string Patronus = "Patronus";
        public const string Vidas    = "Vidas";
    }

    // ?? Nombres de clips de Audio ?????????????????????????????????????????????
    public static class Audio
    {
        // Música
        public const string Escoba   = "Escoba";
        public const string Buckbeak = "Buckbeak";
        public const string Patronus = "Patronus";

        // SFX
        public const string Hit       = "Hit";
        public const string Jump      = "Jump";
        public const string VidaMenos = "VidaMenos";
        public const string Pickup    = "Pickup";
    }

    // ?? Parámetros del Animator (Harry) ???????????????????????????????????????
    public static class AnimatorParams
    {
        public const string Up       = "up";
        public const string Salto    = "salto";
        public const string Escoba   = "escoba";
        public const string Buckbeak = "buckbeak";
        public const string RunLeft  = "runLeft";
        public const string RunRight = "runRight";
        public const string GoLeft   = "goLeft";
        public const string DisparoL = "disparoL";
        public const string DisparoR = "disparoR";

        // Animator (Dementor)
        public const string GoL = "goL";
    }

    // ?? Claves de PlayerPrefs ?????????????????????????????????????????????????
    public static class Prefs
    {
        public const string Highscore   = "highscore";
        public const string VolumenAudio = "volumenAudio";
    }
}