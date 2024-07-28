// See https://aka.ms/new-console-template for more information
using GeneticSharp;

// 1. 定义染色体（10位二进制字符串）
var chromosome = new BinaryChromosome(10);

// 2. 使用自定义的适应度函数
var fitness = new MaxOnesFitness();

// 3. 定义选择策略（精英选择）
var selection = new EliteSelection();

// 4. 定义交叉策略（单点交叉）
var crossover = new OnePointCrossover();

// 5. 定义变异策略（均匀变异）
var mutation = new UniformMutation(true);

// 6. 定义终止条件（适应度达到10或运行100代）
var termination = new OrTermination(
    new FitnessThresholdTermination(10),
    new GenerationNumberTermination(200) // 增加代数
);

// 7. 创建种群
var population = new Population(100, 200, chromosome); // 增加种群规模

// 8. 创建遗传算法
var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
{
    Termination = termination,
    MutationProbability = 0.2f // 增加变异率
};

// 9. 订阅GenerationRan事件
ga.GenerationRan += (sender, e) =>
{
    var bestChromosome = ga.BestChromosome as BinaryChromosome;
    var bestFitness = bestChromosome.Fitness;
    var genes = bestChromosome.GetGenes().Select(g => g.Value).ToArray();
    Console.WriteLine($"代数: {ga.GenerationsNumber}");
    Console.WriteLine($"最佳染色体适应度: {bestFitness}");
    Console.WriteLine($"最佳染色体基因: {string.Join("", genes)}");
};

// 10. 订阅TerminationReached事件
ga.TerminationReached += (sender, e) =>
{
    var bestChromosome = ga.BestChromosome as BinaryChromosome;
    var bestFitness = bestChromosome.Fitness;
    var genes = bestChromosome.GetGenes().Select(g => g.Value).ToArray();
    Console.WriteLine("终止条件达到");
    Console.WriteLine("最佳染色体适应度: {0}", bestFitness);
    Console.WriteLine("最佳染色体基因: {0}", string.Join("", genes));
};

// 11. 开始遗传算法
ga.Start();

internal class BinaryChromosome : ChromosomeBase
{
    public BinaryChromosome(int length) : base(length)
    {
        for (int i = 0; i < length; i++)
        {
            ReplaceGene(i, GenerateGene(i));
        }
    }

    public override IChromosome CreateNew()
    {
        return new BinaryChromosome(Length);
    }

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(RandomizationProvider.Current.GetInt(0, 2));
    }

    public override IChromosome Clone()
    {
        return base.Clone() as BinaryChromosome;
    }
}

public class MaxOnesFitness : IFitness
{
    public double Evaluate(IChromosome chromosome)
    {
        var binaryChromosome = chromosome as BinaryChromosome;
        var genes = binaryChromosome.GetGenes().Select(i => (int)i.Value);
        return genes.Sum();
    }
}