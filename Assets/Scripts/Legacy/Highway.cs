using UnityEngine;


/// <summary>
/// Singleton class containing math functions.
/// </summary>
public class Highway : MonoBehaviour
{

    public const int NUM_LANES = 4;
    public const float LANE_SPACING = 1.9f;
    public const float MID_RADIUS = 31.46f;
    public const float CURVE_LANE0_RADIUS = MID_RADIUS - LANE_SPACING * (NUM_LANES - 1) / 2f;
    public const float MIN_HIGHWAY_LANE0_LENGTH = CURVE_LANE0_RADIUS * 4;

    public GameObject straightPiecePrefab;
    public GameObject curvePiecePrefab;

    private HighwayPiece[] pieces = new HighwayPiece[8];

    public void CreateHighway(float lane0Length)
    {

        if (lane0Length < MIN_HIGHWAY_LANE0_LENGTH)
        {
            Debug.LogError("Highway length must be longer than " + MIN_HIGHWAY_LANE0_LENGTH);
            return;
        }

        float straightPieceLength = (lane0Length - CURVE_LANE0_RADIUS * 4) / 4;

        Vector3 pos = new Vector3(-lane0Length/4 - 2.5f, 0, 0);
        float rot = 0;

        for (int i = 0; i < 8; i++)
        {
            if (i % 2 == 0)
            {
                // straight piece
                if (pieces[i] == null)
                {
                    pieces[i] = Instantiate(straightPiecePrefab, transform).GetComponent<StraightPiece>();
                }

                StraightPiece straightPiece = pieces[i] as StraightPiece;
                straightPiece.SetStartPosition(pos);
                straightPiece.startRotation = rot;
                straightPiece.SetLength(straightPieceLength);

                pos += straightPiece.startRotationQ * new Vector3(0, 0, straightPieceLength);
            }
            else
            {
                // curve piece
                if (pieces[i] == null)
                {
                    pieces[i] = Instantiate(curvePiecePrefab, transform).GetComponent<HighwayPiece>();
                }

                HighwayPiece curvePiece = pieces[i];
                curvePiece.SetStartPosition(pos);
                curvePiece.startRotation = rot;
                pos += curvePiece.startRotationQ * new Vector3(MID_RADIUS, 0, MID_RADIUS);
                rot = Mathf.PI / 2 * (i / 2 + 1);
            }
        }
    }

    private void Start()
    {
        CreateHighway(115);
    }
}