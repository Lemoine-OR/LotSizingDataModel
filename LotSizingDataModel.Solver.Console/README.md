# Console avec export du modèle mathématique

Cette version construit une première fois la formulation standard avant
l'appel au solveur et écrit le modèle solver-independent dans :

C:\Users\david\Documents\test\NouveauFormat\Petit\résolu\mathematical-model.txt

Le dump est donc produit AVANT la résolution CPLEX.

Il contient :
- tous les identifiants et DomainKey des variables ;
- type et bornes de chaque variable ;
- coefficient objectif de chaque variable ;
- objectif complet ;
- regroupement des coefficients objectif par catégorie ;
- toutes les contraintes avec leurs coefficients et leur second membre.

La résolution normale est ensuite effectuée exactement comme auparavant.

Le modèle est volontairement reconstruit une seconde fois par le service de
résolution. Pour ce diagnostic, c'est souhaitable : le dump permet de
contrôler précisément ce que la formulation standard génère indépendamment
de CPLEX.

Installation :
1. Remplacer Program.cs dans LotSizingDataModel.Solver.Console.
2. Ajouter MathematicalModelTextExporter.cs au même projet.
3. Le .csproj fourni peut remplacer l'actuel si besoin.
4. Rebuild Solution.
5. Ctrl+F5.
6. Envoyer ensuite mathematical-model.txt pour analyse.
