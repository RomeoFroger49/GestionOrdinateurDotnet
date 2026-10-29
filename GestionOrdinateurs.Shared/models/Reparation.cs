namespace GestionOrdinateurs.Shared.Models;

public class Reparation
{
    public int Id { get; set; }

    // option 2 pour configurer qu'ordinateurId soit bien une clé primaire de l'entité Ordinateur et clé étragère de l'entité Reparation.
    //[ForeignKey("OrdinateurId")]
    public int OrdinateurId { get; set; }

    public Ordinateur? UnOrdinateur { get; set; }

    public string TypeReparation { get; set; } = "";

    public DateTime DateDebut { get; set; }

    public string Statut { get; set; } = "";
    public string Technicien { get; set; } = "";

    private Reparation() { }

    public Reparation(int ordinateurId, string typeReparation, DateTime dateDebut, string statut, string technicien)
    {
        OrdinateurId = ordinateurId;
        TypeReparation = typeReparation;
        DateDebut = dateDebut;
        Statut = statut;
        Technicien = technicien;
    }
}