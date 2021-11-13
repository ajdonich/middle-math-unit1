using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameNumberHandler : MonoBehaviour
{
    private static Dictionary<TypedNumber.Type, HashSet<TypedNumber.Type>> plateValidNums;
    private static Vector3 defNumStartPos;

    private Vector3 clkOffset = Vector3.zero;
    private TypedNumber.Type hoverPlateId = TypedNumber.Type._UNDEF;
    private TypedNumber typedNumb;

    public AudioClip CorrectDing;
    public AudioClip IncorrectBuzz;

    public static bool validPlateDrop(TypedNumber.Type plateId, TypedNumber tNumber) {
        if (plateValidNums == null) {
            plateValidNums = new Dictionary<TypedNumber.Type, HashSet<TypedNumber.Type>>();
            plateValidNums[TypedNumber.Type._UNDEF] = new HashSet<TypedNumber.Type>();

            plateValidNums[TypedNumber.Type.REAL] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.IRRATIONAL, TypedNumber.Type.RATIONAL, 
                TypedNumber.Type.INTEGER, TypedNumber.Type.WHOLE, TypedNumber.Type.NATURAL});

            plateValidNums[TypedNumber.Type.IRRATIONAL] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.IRRATIONAL});

            plateValidNums[TypedNumber.Type.RATIONAL] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.RATIONAL, TypedNumber.Type.INTEGER, 
                TypedNumber.Type.WHOLE, TypedNumber.Type.NATURAL});

            plateValidNums[TypedNumber.Type.INTEGER] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.INTEGER, TypedNumber.Type.WHOLE, 
                TypedNumber.Type.NATURAL});

            plateValidNums[TypedNumber.Type.WHOLE] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.WHOLE, TypedNumber.Type.NATURAL});

            plateValidNums[TypedNumber.Type.NATURAL] = new HashSet<TypedNumber.Type>(
                new TypedNumber.Type[]{TypedNumber.Type.NATURAL});
        }

        return plateValidNums[plateId].Contains(tNumber.type);
    }

    public void reset() {
        typedNumb = TypedNumber.random();
        transform.position = GameNumberHandler.defNumStartPos;
        GetComponent<TEXDraw3D>().text = typedNumb.snumber;
        // TODO: need to properly adjust size of collider and rect transform
    }

    void Start() {
        defNumStartPos = transform.position;
        reset();
    }

    public void OnMouseDown() {
        clkOffset = Camera.main.WorldToScreenPoint(transform.position) - Input.mousePosition;
    }

    public void OnMouseDrag() {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + clkOffset);
    }

    public void OnMouseUp() {
        if (!validPlateDrop(hoverPlateId, typedNumb))
        {
            if (hoverPlateId != TypedNumber.Type._UNDEF)
                GetComponent<AudioSource>().PlayOneShot(IncorrectBuzz);

            animate_coroutine = AnimatePosition();
            StartCoroutine(animate_coroutine);
        }
        else {
            GetComponent<AudioSource>().PlayOneShot(CorrectDing);
            reset();
        }
    }

    public void OnPlateEnter2D(TypedNumber.Type plateId) {
        hoverPlateId = plateId;
    }

    public void OnPlateExit2D(TypedNumber.Type plateId) {
        if (hoverPlateId == plateId) hoverPlateId = TypedNumber.Type._UNDEF;
    }


    private IEnumerator animate_coroutine = null;
    IEnumerator AnimatePosition(float duration=0.25f) 
    {
        float elapsed = 0;
        Vector3 initPos = transform.position;

        while (elapsed < duration || transform.position != GameNumberHandler.defNumStartPos) {
            transform.position = Vector3.Lerp(initPos, GameNumberHandler.defNumStartPos, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        animate_coroutine = null;
        yield return null;
    }
}
