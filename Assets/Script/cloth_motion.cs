using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cloth_motion : MonoBehaviour
{
    float t = 0.0333f; // 模拟的时间长度
    float mass = 1;
    float damping = 0.99f; // 摩擦系数（模拟空气阻力等）
    float rho = 0.995f;
    float spring_k = 8000;
    int[] E; // 存储每条边的下标
    float[] L; // 每条边的长度
    Vector3[] V;

    private bool m_EnableChebyshevAcceleration = true;

    // 在第一帧更新之前调用 start()
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        // 调整网格大小
        int n = 21;
        Vector3[] X = new Vector3[n * n]; // 441
        Vector2[] UV = new Vector2[n * n];
        int[] triangles = new int[(n - 1) * (n - 1) * 6];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                X[j * n + i] = new Vector3(5 - 10.0f * i / (n - 1), 0, 5 - 10.0f * j / (n - 1));
                // X[j*n+i] =new Vector3(5-10.0f*i/(n-1), -10.0f*j/(n-1),0);
                UV[j * n + i] = new Vector3(i / (n - 1.0f), j / (n - 1.0f));
            }
        int t = 0;
        // 三角化
        for (int j = 0; j < n - 1; j++)
            for (int i = 0; i < n - 1; i++)
            {
                triangles[t * 6 + 0] = j * n + i;
                triangles[t * 6 + 1] = j * n + i + 1;
                triangles[t * 6 + 2] = (j + 1) * n + i + 1;
                triangles[t * 6 + 3] = j * n + i;
                triangles[t * 6 + 4] = (j + 1) * n + i + 1;
                triangles[t * 6 + 5] = (j + 1) * n + i;
                t++;
            }
        mesh.vertices = X;
        mesh.triangles = triangles;
        mesh.uv = UV;
        mesh.RecalculateNormals();

        int[] _E = new int[triangles.Length * 2];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            _E[i * 2 + 0] = triangles[i + 0];
            _E[i * 2 + 1] = triangles[i + 1];
            _E[i * 2 + 2] = triangles[i + 1];
            _E[i * 2 + 3] = triangles[i + 2];
            _E[i * 2 + 4] = triangles[i + 2];
            _E[i * 2 + 5] = triangles[i + 0];
        }
        for (int i = 0; i < _E.Length; i += 2)
            if (_E[i] > _E[i + 1])
                Swap(ref _E[i], ref _E[i + 1]);
        Quick_Sort(ref _E, 0, _E.Length / 2 - 1);

        int e_number = 0;
        for (int i = 0; i < _E.Length; i += 2)
            if (i == 0 || _E[i + 0] != _E[i - 2] || _E[i + 1] != _E[i - 1])
                e_number++;

        E = new int[e_number * 2];
        for (int i = 0, e = 0; i < _E.Length; i += 2)
            if (i == 0 || _E[i + 0] != _E[i - 2] || _E[i + 1] != _E[i - 1])
            {
                E[e * 2 + 0] = _E[i + 0];
                E[e * 2 + 1] = _E[i + 1];
                e++;
            }

        // 计算弹簧原长度
        L = new float[E.Length / 2];
        for (int e = 0; e < E.Length / 2; e++)
        {
            int v0 = E[e * 2 + 0];
            int v1 = E[e * 2 + 1];
            L[e] = (X[v0] - X[v1]).magnitude;
        }

        V = new Vector3[X.Length];
        for (int i = 0; i < V.Length; i++)
            V[i] = new Vector3(0, 0, 0);
    }

    void Quick_Sort(ref int[] a, int l, int r)
    {
        int j;
        if (l < r)
        {
            j = Quick_Sort_Partition(ref a, l, r);
            Quick_Sort(ref a, l, j - 1);
            Quick_Sort(ref a, j + 1, r);
        }
    }

    int Quick_Sort_Partition(ref int[] a, int l, int r)
    {
        int pivot_0,
            pivot_1,
            i,
            j;
        pivot_0 = a[l * 2 + 0];
        pivot_1 = a[l * 2 + 1];
        i = l;
        j = r + 1;
        while (true)
        {
            do ++i;
            while (
                i <= r && (a[i * 2] < pivot_0 || a[i * 2] == pivot_0 && a[i * 2 + 1] <= pivot_1)
            );
            do --j;
            while (a[j * 2] > pivot_0 || a[j * 2] == pivot_0 && a[j * 2 + 1] > pivot_1);
            if (i >= j)
                break;
            Swap(ref a[i * 2], ref a[j * 2]);
            Swap(ref a[i * 2 + 1], ref a[j * 2 + 1]);
        }
        Swap(ref a[l * 2 + 0], ref a[j * 2 + 0]);
        Swap(ref a[l * 2 + 1], ref a[j * 2 + 1]);
        return j;
    }

    void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    void Collision_Handling()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;

        //Handle colllision.
        Vector3 c = GameObject.Find("Sphere").transform.position;
        // 碰撞检测，并将碰撞的点沿球心与点的方向移动
        for (int i = 0; i < V.Length; i++)
        {
            float distance = (X[i] - c).magnitude;
            if (distance < 2.7f)
            {
                Vector3 direction = (X[i] - c) / distance;
                V[i] += 1.0f / t * (c + 2.7f * direction - X[i]);
                X[i] = c + 2.7f * direction;
            }
        }

        mesh.vertices = X;
    }

    void Get_Gradient(Vector3[] X, Vector3[] X_hat, float t, Vector3[] G)
    {
        // 动量和重力
        Vector3 gravity = new Vector3(0, -9.8f, 0);
        Vector3 wind = Vector3.zero;
        if (Time.frameCount % 300 < 100 + 100 * Random.value)
        {
            wind = new Vector3(0, 0, -1) * Random.value * 0.5f;
        }

        // 根据点的移动，模拟力的大小，包括力与重力。
        for (int i = 0; i < G.Length; i++)
        {
            G[i] = (1.0f / t / t) * mass * (X[i] - X_hat[i]) - (gravity + wind) * mass;
        }

        // 模拟弹簧的力
        for (int e = 0; e < E.Length / 2; e++)
        {
            int i = E[e * 2];
            int j = E[e * 2 + 1];
            Vector3 springForce = spring_k * (1 - L[e] / (X[i] - X[j]).magnitude) * (X[i] - X[j]);
            G[i] += springForce;
            G[j] -= springForce;
        }
    }

    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] X = mesh.vertices;
        Vector3[] last_X = new Vector3[X.Length];
        Vector3[] X_hat = new Vector3[X.Length];
        Vector3[] G = new Vector3[X.Length];

        for (int i = 0; i < V.Length; ++i)
        {
            V[i] *= damping;
            X_hat[i] = X[i] + t * V[i];
            // X[i] = X_hat[i];
        }

        float w = 0;
        for (int k = 0; k < 32; k++)
        {
            Get_Gradient(X, X_hat, t, G);

            if (m_EnableChebyshevAcceleration)
            {
                if (k == 0)
                    w = 1;
                else if (k == 1)
                    w = 2 / (2 - rho * rho);
                else
                    w = 4 / (4 - rho * rho * w);

                for (int i = 0; i < X.Length; i++)
                {
                    if (i == 0 || i == 20)
                        continue;
                    Vector3 xOld = X[i];
                    X[i] -= 1.0f / ((1.0f / t / t) * mass + 4.0f * spring_k) * G[i];
                    X[i] = w * X[i] + (1 - w) * last_X[i];
                    last_X[i] = xOld;
                }
            }
            else
            {
                // 根据力的大小更新位移
                for (int i = 0; i < X.Length; i++)
                {
                    if (i == 0 || i == 20)
                        continue;
                    X[i] -= 1.0f / ((1.0f / t / t) * mass + 4.0f * spring_k) * G[i];
                }
            }
        }

        // 更新速度
        for (int i = 0; i < V.Length; i++)
        {
            if (i == 0 || i == 20)
                continue;
            V[i] += 1.0f / t * (X[i] - X_hat[i]);
        }

        mesh.vertices = X;

        Collision_Handling();
        mesh.RecalculateNormals();
    }
}
