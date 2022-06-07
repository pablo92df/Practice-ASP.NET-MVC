using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones
{
    //validaciones personalizadas. Siempre lleva la palabra attribute el nombre
    public class PrimeraLetraMayusculaAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) 
            {
                return ValidationResult.Success;
            }

            var primeraLetra = value.ToString()[0].ToString();

            if (primeraLetra != primeraLetra.ToUpper()) 
            {
                return new ValidationResult("La primera letra debe ser mayuscula");
            }
            return ValidationResult.Success;
        }
    }
}
