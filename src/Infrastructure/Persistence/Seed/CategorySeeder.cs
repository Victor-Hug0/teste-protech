using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed;

public static class CategorySeeder
{
    private static readonly (string Name, string Description)[] DefaultCategories =
    [
        ("Eletrônicos", "Smartphones, notebooks, TVs e acessórios eletrônicos."),
        ("Informática", "Computadores, periféricos, componentes e software."),
        ("Moda e Vestuário", "Roupas, calçados e acessórios de moda."),
        ("Beleza e Perfumaria", "Cosméticos, perfumes e cuidados pessoais."),
        ("Casa e Decoração", "Móveis, utilidades domésticas e decoração."),
        ("Eletrodomésticos", "Geladeiras, fogões, lavadoras e pequenos eletros."),
        ("Esportes e Lazer", "Artigos esportivos, fitness e lazer ao ar livre."),
        ("Livros e Papelaria", "Livros, materiais escolares e papelaria."),
        ("Brinquedos e Games", "Brinquedos, jogos de tabuleiro e videogames."),
        ("Alimentos e Bebidas", "Mercearia, bebidas e produtos gourmet."),
        ("Saúde e Bem-estar", "Suplementos, farmácia e bem-estar."),
        ("Pet Shop", "Ração, acessórios e produtos para animais."),
        ("Automotivo", "Peças, acessórios e cuidados com veículos."),
        ("Ferramentas e Construção", "Ferramentas, materiais de construção e jardinagem."),
        ("Joias e Relógios", "Joias, bijuterias e relógios.")
    ];

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
            return;

        foreach (var (name, description) in DefaultCategories)
            await context.Categories.AddAsync(Category.Create(name, description), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
