using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class TestBurstBug : MonoBehaviour
{
    public struct Data : IComponentData
    {
        public MyVector Value1;
        public MyVector Value2;
    }

    public struct MyVector
    {
        public long X;
        public long Y;
        public long Z;

        public override string ToString()
        {
            return $"({X})";
        }
    }

    public static MyVector UpdateMyVector(MyVector value)
    {
        value = new MyVector
        {
            X = value.X + 5678,
        };
        return value;
    }

    public class TestBurstComponentSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct Job : IJobProcessComponentData<Data>
        {
            public void Execute(ref Data data)
            {
                data.Value2 = UpdateMyVector(data.Value1);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new Job().Schedule(this, inputDeps);
        }
    }

    [UpdateAfter(typeof(TestBurstComponentSystem))]
    public class OutputSystem : ComponentSystem
    {
        private struct Input
        {
            public readonly int Length;
            public ComponentDataArray<Data> DataArray;
        }

        [Inject] private Input m_input;

        public TestBurstBug TestBurstBug;

        protected override void OnUpdate()
        {
            for (var i = 0; i < m_input.Length; i++)
            {
                var data = m_input.DataArray[i];
                TestBurstBug.OutputText.text = $"{data.Value2}";
            }
        }
    }

    public Text OutputText;

    private void Start()
    {
        var manager = World.Active.GetOrCreateManager<EntityManager>();
        var e = manager.CreateEntity();
        manager.AddComponentData(e, new Data());

        World.Active.GetOrCreateManager<TestBurstComponentSystem>();
        World.Active.GetOrCreateManager<OutputSystem>().TestBurstBug = this;
    }
}