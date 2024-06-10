using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

public class CameraActive : MonoBehaviour {
    public bool IsReset;
    public Transform Player;
    public Transform Camera;
    public float Fade = 0.25f;
    public float Speed = 6f;

    public List<Material> MateList;

    private Quaternion camRotation;
    private Vector3 camOffset;
    private Vector3 camTarget;
    private int layerMask;

    private void CameraHitCollider() {
        var state = Physics.Raycast(Player.transform.position, camTarget, out var hit, 5f, layerMask);
        if (state && hit.transform.CompareTag("Solid")) {
            var zPos = Vector3.Distance(transform.position, hit.point);
            Camera.localPosition = new Vector3(camOffset.x, camOffset.y, -zPos);
        } else {
            Camera.localPosition = Vector3.Lerp(Camera.localPosition, camOffset, Time.deltaTime);
        }
    }

    private void CameraHitAlpha() {
        var hits = Physics.RaycastAll(Player.transform.position, camTarget, 5f, layerMask);
        if (hits.Length > 0) {
            for (int i = 0; i < hits.Length; i++) {
                if (hits[i].collider.CompareTag("Ambient")) {
                    var mate = hits[i].transform.GetComponent<Renderer>().material;
                    var mateCheck = MateList.Contains(mate);
                    if (!mateCheck) {
                        CameraFadeAlpha(mate, false);
                        MateList.Add(mate);
                    }
                    foreach (var item in MateList) {
                        if (mate == item) {
                            mate.color = AlphaAdjustment(mate, Speed / 2, Fade);
                        }
                    }
                }
            }
        } else {
            MaterialNormal();
        }
    }

    private void MaterialNormal() {
        for (int i= 0; i < MateList.Count; i++) {
            CameraFadeAlpha(MateList[i], true);
            MateList[i].color = AlphaAdjustment(MateList[i], Speed / 2);
            if (MateList[i].color.a > 0.9f) {
                MateList.Remove(MateList[i]);
            }
        }
    }

    //private void CameraHitCollider(RaycastHit hit, bool state, string tag) {
    //    if (state && hit.collider.CompareTag(tag)) {
    //        var zPos = Vector3.Distance(transform.position, hit.point);
    //        Camera.localPosition = new Vector3(camOffset.x, camOffset.y, -zPos);
    //    } else {
    //        Camera.localPosition = Vector3.Lerp(Camera.localPosition, camOffset, Time.deltaTime);
    //    }
    //}

    //private void CameraHitAlpha(RaycastHit hit, bool state, string tag) {
    //    if (hit.transform) {
    //        Debug.Log(hit.transform.name ?? "Undefined");
    //    }


    //    if (state && hit.collider.CompareTag(tag)) {


    //        var mate = hit.transform.GetComponent<Renderer>().material;
    //        var mateCheck = MateList.Contains(mate);
    //        if (!mateCheck) {
    //            //mate.SetFloat("_Mode", 3);
    //            //mate.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
    //            //mate.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
    //            //mate.SetInt("_ZWrite", 0);
    //            //mate.DisableKeyword("_ALPHATEST_ON");
    //            //mate.EnableKeyword("_ALPHABLEND_ON");
    //            //mate.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //            //mate.renderQueue = 3000;
    //            CameraFadeAlpha(mate, false);
    //            MateList.Add(mate);
    //        }

    //        foreach (var item in MateList) {
    //            if (mate == item) {
    //                //var alpha = Mathf.Lerp(item.color.a, 0.1f, 5f * Time.deltaTime);
    //                //mate.color = new Color(item.color.r, item.color.g, item.color.b, alpha);
    //                mate.color = AlphaAdjustment(mate, Speed / 2, Fade);
    //            }
    //        }
    //    } else {
    //        for (int i = 0; i < MateList.Count; i++) {
    //            if (MateList[i].color.a < 0.9f) {
    //                //MateList[i].SetFloat("_Mode", 0);
    //                //MateList[i].SetInt("_SrcBlend", (int)BlendMode.One);
    //                //MateList[i].SetInt("_DstBlend", (int)BlendMode.Zero);
    //                //MateList[i].SetInt("_ZWrite", 1);
    //                //MateList[i].DisableKeyword("_ALPHATEST_ON");
    //                //MateList[i].EnableKeyword("_ALPHABLEND_ON");
    //                //MateList[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //                //MateList[i].renderQueue = -1;
    //                CameraFadeAlpha(MateList[i], true);
    //                //var alpha = Mathf.Lerp(MateList[i].color.a, 1f, 5f * Time.deltaTime);
    //                //MateList[i].color = new Color(MateList[i].color.r, MateList[i].color.g, MateList[i].color.b, alpha);
    //                MateList[i].color = AlphaAdjustment(MateList[i], Speed / 2);
    //                if (MateList[i].color.a > 0.9f) {
    //                    MateList.Remove(MateList[i]);
    //                }
    //            }
    //        }
    //    }
    //}

    private void CameraFadeAlpha(Material mate, bool opaque) {
        mate.SetFloat("_Mode", opaque ? 0 : 3);
        mate.SetInt("_SrcBlend", (int)(opaque ? BlendMode.One : BlendMode.SrcAlpha));
        mate.SetInt("_DstBlend", (int)(opaque ? BlendMode.Zero : BlendMode.OneMinusSrcAlpha));
        mate.SetInt("_ZWrite", opaque ? 1 : 0);
        mate.DisableKeyword("_ALPHATEST_ON");
        if (opaque) {
            mate.DisableKeyword("_ALPHABLEND_ON");
        } else {
            mate.EnableKeyword("_ALPHABLEND_ON");
        }
        mate.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mate.renderQueue = opaque ? -1 : 3000;
    }

    private Color AlphaAdjustment(Material mate, float speed, float deep = 1f) {
        var alpha = Mathf.Lerp(mate.color.a, deep, speed * Time.deltaTime);
        return new Color(mate.color.r, mate.color.g, mate.color.b, alpha);
    }

    private void Start() {
        camRotation = transform.localRotation;
        camOffset = Camera.localPosition;
        layerMask = 1 << 7;  //----------
        MateList = new();
    }

    private void Update() {
        var input = PlayerActive.Instance.InputPlay;
        var vector = input ? input.LookHandler.normalized : Vector3.zero;
        //var target = Player.transform.position + transform.localRotation * camOffset;
        //var state = Physics.Raycast(Player.transform.position, target, out var hit);
        //var obj = Physics.RaycastAll(Player.transform.position, target);


        camTarget = Camera.position - Player.transform.position;
        Debug.DrawRay(Player.transform.position, camTarget);
        
        CameraHitCollider();
        CameraHitAlpha();

        //CameraHitCollider(hit, state, "Solid");
        //CameraHitAlpha(hit, state, "Ambient");

        Camera.rotation = Quaternion.LookRotation(Player.position - Camera.position);
        transform.position = Vector3.Slerp(transform.position, Player.position, Speed * Time.deltaTime);

        camRotation.x += vector.y;
        camRotation.y += vector.x;
        camRotation.x = Mathf.Clamp(camRotation.x, -15f, 45f);
        transform.localRotation = Quaternion.Euler(camRotation.x, camRotation.y, camRotation.z);

    }
}