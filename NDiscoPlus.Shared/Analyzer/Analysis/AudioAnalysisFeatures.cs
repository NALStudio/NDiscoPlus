using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer.Analysis;
internal class AudioAnalysisFeatures
{
    /// <summary>
    /// <para>A confidence measure from 0.0 to 1.0 of whether the track is acoustic.</para>
    /// <para>1.0 represents high confidence the track is acoustic.</para>
    /// </summary>
    public float Acousticness { get; }

    /// <summary>
    /// <para>Danceability describes how suitable a track is for dancing based on a combination of musical elements including tempo, rhythm stability, beat strength, and overall regularity.</para>
    /// <para>A value of 0.0 is least danceable and 1.0 is most danceable.</para>
    /// </summary>
    public float Danceability { get; }

    /// <summary>
    /// <para>Energy is a measure from 0.0 to 1.0 and represents a perceptual measure of intensity and activity.</para>
    /// <para>Typically, energetic tracks feel fast, loud, and noisy. For example, death metal has high energy, while a Bach prelude scores low on the scale.</para>
    /// <para>Perceptual features contributing to this attribute include dynamic range, perceived loudness, timbre, onset rate, and general entropy.</para>
    /// </summary>
    public float Energy { get; }

    /// <summary>
    /// <para>Predicts whether a track contains no vocals. "Ooh" and "aah" sounds are treated as instrumental in this context. Rap or spoken word tracks are clearly "vocal".</para>
    /// <para>The closer the instrumentalness value is to 1.0, the greater likelihood the track contains no vocal content.</para>
    /// <para>Values above 0.5 are intended to represent instrumental tracks, but confidence is higher as the value approaches 1.0.</para>
    /// </summary>
    public float Instrumentalness { get; }

    /// <summary>
    /// <para>Detects the presence of an audience in the recording.</para>
    /// <para>Higher liveness values represent an increased probability that the track was performed live.</para>
    /// <para>A value above 0.8 provides strong likelihood that the track is live.</para>
    /// </summary>
    public float Liveness { get; }

    /// <summary>
    /// <para>The overall loudness of a track in decibels (dB).</para>
    /// <para>Loudness values are averaged across the entire track and are useful for comparing relative loudness of tracks.</para>
    /// <para>Loudness is the quality of a sound that is the primary psychological correlate of physical strength (amplitude).</para>
    /// <para>Values typically range between -60 and 0 db.</para>
    /// </summary>
    public float Loudness { get; }

    /// <summary>
    /// <para>Speechiness detects the presence of spoken words in a track.</para>
    /// <para>The more exclusively speech-like the recording (e.g. talk show, audio book, poetry), the closer to 1.0 the attribute value.</para>
    /// <para>Values above 0.66 describe tracks that are probably made entirely of spoken words.</para>
    /// <para>Values between 0.33 and 0.66 describe tracks that may contain both music and speech, either in sections or layered, including such cases as rap music.</para>
    /// <para>Values below 0.33 most likely represent music and other non-speech-like tracks.</para>
    /// </summary>
    public float Speechiness { get; }

    /// <summary>
    /// <para>A measure from 0.0 to 1.0 describing the musical positiveness conveyed by a track.</para>
    /// <para>Tracks with high valence sound more positive (e.g. happy, cheerful, euphoric), while tracks with low valence sound more negative (e.g. sad, depressed, angry).</para>
    /// </summary>
    public float Valence { get; }

    private AudioAnalysisFeatures(SpotifyAPI.Web.TrackAudioFeatures features)
    {
        Acousticness = features.Acousticness;
        Danceability = features.Danceability;
        Energy = features.Energy;
        Instrumentalness = features.Instrumentalness;
        Liveness = features.Liveness;
        Loudness = features.Loudness;
        Speechiness = features.Speechiness;
        Valence = features.Valence;
    }

    public static AudioAnalysisFeatures FromSpotify(SpotifyAPI.Web.TrackAudioFeatures features)
        => new(features);
}
