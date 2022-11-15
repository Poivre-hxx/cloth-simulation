using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ball_connection : MonoBehaviour
{
    public List<Rigidbody2D> Nodelist;
    public List<HingeJoint2D> NodelistHj;
    public Transform node;//节点
    public int num = 3;//节点数量
    public float NodeDis = 1;//间距
    public LineRenderer line;
    [Button("init")]
    public void Init() {
        Nodelist = new List<Rigidbody2D>();
        NodelistHj = new List<HingeJoint2D>();
        for (int i = 0; i < num; i++) {
            Transform n = transform.Find(i.toString());
            if (!n) {
                n = GameObject.Instantiate(node);
                n.name = i.toString();
                n.parent = transform;
                n.gameObject.setActive(true);
            }
            n.localPosition = Vector3.left * i * NodeDis;
            var rig = n.GetComponent<Rigidbody2D>();
            rig.bodyType = RigidbodyType2D.Static;
            rig.gravityScale = 2;
            rig.angularDrag = 1f;
            rig.drag = 0.5f;
            Nodelist.Add(rig);

        }
        for (int i = 0; i < Nodelist.Count; i++) {
            var n = Nodelist[i];
            var hj = n.GetComponent<HingeJoint2D>();
            if (i != Nodelist.Count - 1) {
                hj.connectedBody = Nodelist[i + 1];
                hj.connectedAnchor = new Vector2(0, -NodeDis);
                hj.enabled = true;
            }
            else {
                hj.enabled = false;
            }
            NodelistHj.Add(hj);
        }
    }
    public void AddF(bool isadd) {
        if (isadd) {
            for (int i = seid; i < Nodelist.Count; i++) {
                var n = Nodelist[i];
                n.drag = 0;
            }
        }
        else {
            for (int i = seid; i < Nodelist.Count; i++) {
                var n = Nodelist[i];
                n.drag = 0.5f;
            }
        }
    }


    [Button("开始")]
    public void Begin() {
        for (int i = 1; i < Nodelist.Count; i++) {
            Nodelist[i].bodyType = RigidbodyType2D.Dynamic;
        }
    }
    void Start() {
        line = transform.GetOrAddComponent<LineRenderer>();
    }
    public int seid;
    public float f = 10;
    void Update() {
        var ypos = transform.position.toVector2();
        if (Input.GetKey(KeyCode.A)) {
            var cur = Nodelist[seid];
            cur.AddForce(Vector2.left * f * seid);
            cur.AddForce((cur.position - ypos).normalized * f * seid * 3);
        }
        else if (Input.GetKey(KeyCode.D)) {
            var cur = Nodelist[seid];
            cur.AddForce(Vector2.right * f * seid);
            cur.AddForce((cur.position - ypos).normalized * f * seid * 3);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) {
            AddF(true);
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) {
            AddF(false);
        }

        if (Input.GetKey(KeyCode.W)) {

            NodeDis += (NodeDis / 100);
            for (int i = 0; i < NodelistHj.Count; i++) {
                var n = NodelistHj[i];
                n.connectedAnchor = new Vector2(0, -NodeDis);
            }
        }
        else if (Input.GetKey(KeyCode.S)) {

            NodeDis -= (NodeDis / 100);
            if (NodeDis < 0) {
                NodeDis = 0;
            }
            else {
                for (int i = 0; i < NodelistHj.Count; i++) {
                    var n = NodelistHj[i];
                    n.connectedAnchor = new Vector2(0, -NodeDis);
                }
            }
        }
    }

    private void FixedUpdate() {
        var ypos = transform.position.toVector2();
        //圈内约束
        for (int i = 1; i < Nodelist.Count; i++) {
            var c = Nodelist[i];
            var cpos = c.position;//当前位置
            var cdir = c.velocity;//当前速度
            var npos = cpos + cdir * Time.fixedDeltaTime;//下一帧到的位置
            var nydis = Vector2.Distance(npos, ypos);//下一帧与原点的距离

            debug.Draw().Line(cpos, npos);
            debug.Draw().Line(ypos, npos);

            if (nydis > i * NodeDis) {//超出当前这个点的最大距离
                var ydir = (npos - ypos).normalized;//原点到新点的方向
                var rpos = ypos + ydir * i * NodeDis;//的到修正点位置
                var rdir = rpos - cpos;//修正点的方向
                c.velocity = (rdir / Time.fixedDeltaTime);//修正的速度 

                debug.Draw().Line(cpos, rpos, Color.green);
            }
        }
        debug.stop();
    }

    private void LateUpdate() {
        line.positionCount = Nodelist.Count;
        for (int i = 0; i < Nodelist.Count; i++) {
            line.SetPosition(i, Nodelist[i].position);
        }
    }


    private void OnDrawGizmos() {
        for (int i = 0; i < num; i++) {
            debug.Draw().Circle(transform.position, i * NodeDis);
        }
        for (int i = 0; i < Nodelist.Count; i++) {
            var node = Nodelist[i];
            debug.Draw().Ray(node.transform.position, node.velocity, Mathf.Sqrt(node.velocity.sqrMagnitude), Color.yellow);
        }
    }

}
