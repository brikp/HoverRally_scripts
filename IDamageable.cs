using UnityEngine;
using System.Collections;

//every obcject that could take damege and get kill should implements this interface
public interface IDamageable <T> 
{
	void Damage(T damageTaken);

	void Kill();
}
