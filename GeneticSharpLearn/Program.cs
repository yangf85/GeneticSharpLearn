// See https://aka.ms/new-console-template for more information
using GeneticSharp;

int[] stockLengths = { 100, 100 }; // 两根100长度的木条
int[] demandLengths = { 45, 35, 20, 10, 5 }; // 需求长度
int[] demandQuantities = { 2, 1, 2, 3, 5 }; // 每种需求的数量

// 计算初始种群数量
int totalDemandCount = demandQuantities.Sum();
var chromosome = new CustomCuttingChromosome(stockLengths, demandLengths, demandQuantities);
var population = new Population(totalDemandCount, totalDemandCount, chromosome);
var fitness = new CustomCuttingFitness(stockLengths, demandLengths, demandQuantities);
var selection = new EliteSelection();
var crossover = new OnePointCrossover();
var mutation = new TworsMutation();
var termination = new GenerationNumberTermination(1000);

var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
{
    Termination = termination
};

ga.GenerationRan += (sender, e) =>
{
    var bestChromosome = ga.BestChromosome as CustomCuttingChromosome;
    Console.WriteLine($"Generation {ga.GenerationsNumber}: Best solution: {string.Join(", ", bestChromosome.GetGenes().Select(g => g.Value))}");
};

ga.Start();

public class CustomCuttingChromosome : ChromosomeBase
{
    private readonly int[] _stockLengths;

    private readonly int[] _demandLengths;

    private readonly int[] _demandQuantities;

    public CustomCuttingChromosome(int[] stockLengths, int[] demandLengths, int[] demandQuantities)
        : base(demandLengths.Sum(d => demandQuantities[Array.IndexOf(demandLengths, d)])) // 设置染色体长度
    {
        _stockLengths = stockLengths;
        _demandLengths = demandLengths;
        _demandQuantities = demandQuantities;

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        var randomIndex = RandomizationProvider.Current.GetInt(0, _demandLengths.Length);
        return new Gene(_demandLengths[randomIndex]);
    }

    public override IChromosome CreateNew()
    {
        return new CustomCuttingChromosome(_stockLengths, _demandLengths, _demandQuantities);
    }

    private void CreateGenes()
    {
        var geneList = new List<int>();

        // 将需求长度按数量放入基因列表
        for (int i = 0; i < _demandLengths.Length; i++)
        {
            for (int j = 0; j < _demandQuantities[i]; j++)
            {
                geneList.Add(_demandLengths[i]);
            }
        }

        // 随机打乱基因列表
        geneList = geneList.OrderBy(x => Guid.NewGuid()).ToList();

        // 替换基因
        for (int i = 0; i < geneList.Count; i++)
        {
            ReplaceGene(i, new Gene(geneList[i]));
        }
    }
}

public class CustomCuttingFitness : IFitness
{
    private readonly int[] _stockLengths;

    private readonly int[] _demandLengths;

    private readonly int[] _demandQuantities;

    public CustomCuttingFitness(int[] stockLengths, int[] demandLengths, int[] demandQuantities)
    {
        _stockLengths = stockLengths;
        _demandLengths = demandLengths;
        _demandQuantities = demandQuantities;
    }

    public double Evaluate(IChromosome chromosome)
    {
        var genes = chromosome.GetGenes().Select(g => (int)g.Value).ToList();
        int totalWaste = 0;
        int usedStocks = 0;

        var demandCounts = new int[_demandLengths.Length];
        foreach (var gene in genes)
        {
            int index = Array.IndexOf(_demandLengths, gene);
            if (index >= 0)
            {
                demandCounts[index]++;
            }
        }

        // 检查是否满足所有需求
        for (int i = 0; i < _demandLengths.Length; i++)
        {
            if (demandCounts[i] < _demandQuantities[i])
            {
                return 0; // 不满足所有需求则返回最低适应度
            }
        }

        // 计算木条切割浪费和使用的木条数量
        foreach (var stockLength in _stockLengths)
        {
            int currentLength = 0;
            foreach (var gene in genes)
            {
                if (currentLength + gene <= stockLength)
                {
                    currentLength += gene;
                }
                else
                {
                    totalWaste += (stockLength - currentLength);
                    currentLength = gene;
                    usedStocks++;
                }
            }

            totalWaste += (stockLength - currentLength);
            usedStocks++;
        }

        return 1.0 / (1 + totalWaste + usedStocks);
    }
}