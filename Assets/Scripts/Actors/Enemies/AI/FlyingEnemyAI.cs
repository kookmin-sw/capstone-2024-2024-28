using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utils;


[RequireComponent(typeof(Attacker))]
public class FlyingEnemyAI : EnemyAI
{
    private GridDetector grid_detector = null;
    private Vector2Int current_move = Vector2Int.zero;
    private float update_time = 0.0625f;
    private float current_time = 0.0f;
    private float last_moved_time = 0.0f;
    private readonly object stop_lock = new object();
    private bool is_attacking = false;
    private Attacker attacker = null;

    private readonly Vector2Int[] CARDINAL_DIR = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    protected override void Awake()
    {
        base.Awake();
        grid_detector = FindObjectOfType<GridDetector>();
        attacker = GetComponent<RangeAttacker>();
        last_moved_time = Time.time;
    }

    private List<Vector2Int> backtrack_path_finding(Dictionary<Vector2Int, Vector2Int> last_pos, Vector2Int dest)
    {
        List<Vector2Int> path = new();

        Vector2Int pos = dest;
        while (true)
        {
            Vector2Int last = last_pos[pos];
            if (last == pos)
                break;

            path.Add(pos - last);
            pos = last;
        }
        path.Reverse();

        return path;
    }

    public List<Vector2Int> find_path()
    {
        Vector2Int player_pos = grid_detector.get_cell(Locator.player.transform.position);
        if (grid_detector.is_tile(player_pos))
            return null;

        Vector2Int current_pos = grid_detector.get_cell(transform.position);
        if ((player_pos - current_pos).magnitude > 100.0f)
            return null;

        Dictionary<Vector2Int, Vector2Int> last_pos = new();
        PriorityQueue<(Vector2Int last, Vector2Int current, float cost), float> pq = new();
        pq.Enqueue((current_pos, current_pos, 0.0f), 0);
        while (pq.Count > 0)
        {
            (Vector2Int last, Vector2Int current, float cost) = pq.Dequeue();
            if (cost > 300.0f || pq.Count > 300.0f)
                break;
                
            if (last_pos.ContainsKey(current))
                continue;
            last_pos[current] = last;
            
            if (current == player_pos)
                return backtrack_path_finding(last_pos, current);

            for (int i = 0; i < CARDINAL_DIR.Length; i++)
            {
                Vector2Int dir = CARDINAL_DIR[i];
                Vector2Int next_dir = CARDINAL_DIR[(i + 1) % CARDINAL_DIR.Length];

                bool enqueue(Vector2Int pos, float weight)
                {
                    if (grid_detector.is_tile(pos))
                        return false;

                    float next_cost = cost + weight;
                    float h_cost = 5.0f * (pos - player_pos).magnitude;
                    pq.Enqueue(
                        (current, pos, next_cost),
                        next_cost + h_cost
                    );
                    return true;
                }

                if (!enqueue(current + dir, 1.0f))
                    continue;

                if (!grid_detector.is_tile(current + next_dir))
                    enqueue(current + dir + next_dir, Mathf.Sqrt(2.0f));
            }
        }
        
        return null;
    }

    private Vector2 find_blank_dir()
    {
        Vector2Int current = grid_detector.get_cell(transform.position);

        foreach (var dir in CARDINAL_DIR.Concat(new Vector2Int[]{ Vector2Int.zero }))
        {
            Vector2Int pos = current + dir;
            if (!grid_detector.is_tile(pos))
                return pos - (Vector2)transform.position;
        }
        return current;
    }

    private void move_dir(Vector2 dir)
    {
        actor_controller.move(dir);
        last_moved_time = Time.time;
    }

    private void update_move()
    {
        if (Time.time - last_moved_time > 1.0f) {
            move_dir(find_blank_dir());
            return;
        }

        Vector3 position = transform.position;
        float dx = Mathf.Abs(Mathf.Round(position.x) - position.x);
        float dy = Mathf.Abs(Mathf.Round(position.y) - position.y);
        if (dx <= 0.35f || dy <= 0.35f)
            return;

        var path = find_path();
        if (path is null || path.Count == 0)
            move_dir(Vector2.zero);
        else
            move_dir(path[0]);
    }

    public void on_finish_attack()
    {
        is_attacking = false;
    }
    
    void FixedUpdate()
    {
        if (is_attacking)
            return;

        current_time += Time.deltaTime;
        if (current_time < update_time)
            return;
        current_time -= update_time;
        update_move();
        
        if (attacker.check()) {
            SendMessage("on_start_attack", SendMessageOptions.DontRequireReceiver);
            is_attacking = true;
            actor_controller.move(Vector2.zero);
        }
    }
}